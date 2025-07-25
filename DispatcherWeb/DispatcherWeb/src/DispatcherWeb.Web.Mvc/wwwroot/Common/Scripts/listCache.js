/// <reference path="../../../node_modules/@dumptruckdispatcher/datatables-helper/typings/moment.d.ts" />
/// <reference path="../../lib/idb/entry.d.ts" />
(function () {
    // Import idb functionality
    const { openDB, deleteDB } = window.idb;
    const CacheCodeVersion = 8;
    const DB_NAME = 'dtdListCache';
    const DB_VERSION = 1;
    const DB_STORES_CACHE_ITEM = 'cacheItems';
    // DB initialization promise
    let dbPromise = initDatabase();
    // Initialize IndexedDB
    async function initDatabase() {
        return openDB(DB_NAME, DB_VERSION, {
            upgrade(db, oldVersion, newVersion, transaction) {
                if (oldVersion < 1) {
                    if (!db.objectStoreNames.contains(DB_STORES_CACHE_ITEM)) {
                        const store = db.createObjectStore(DB_STORES_CACHE_ITEM, {
                            keyPath: ['cacheName', 'keyString', 'cacheCodeVersion', 'globalCacheVersion'],
                        });
                        // Create indexes for easier querying
                        store.createIndex('by-cache-name', 'cacheName');
                        store.createIndex('by-last-accessed-at', 'lastAccessedAt');
                        store.createIndex('by-is-last-accessed', 'isLastAccessed');
                    }
                }
            },
            blocked() {
                console.warn('Database upgrade was blocked');
            },
            blocking() {
                console.warn('Current database is blocking a newer version');
            },
            terminated() {
                console.error('Database was terminated unexpectedly');
            }
        });
    }
    class AsyncLock {
        locks = new Map();
        async withLock(key, operation) {
            // Check if there's already an active lock
            const existingLock = this.locks.get(key);
            if (existingLock) {
                // Wait for the existing operation to complete
                await existingLock;
                // After waiting, try again (recursive) - this ensures proper queuing
                return this.withLock(key, operation);
            }
            // Create a new lock promise
            let resolveLock;
            const lockPromise = new Promise(resolve => {
                resolveLock = resolve;
            });
            // Register the lock
            this.locks.set(key, lockPromise);
            try {
                // Execute the operation
                const result = await operation();
                return result;
            }
            finally {
                // Release the lock
                resolveLock();
                this.locks.delete(key);
            }
        }
        isLocked(key) {
            return this.locks.has(key);
        }
    }
    let listKeyFormatters = {
        emptyKey: {
            isTenantDependent: false,
            isHardInvalidatable: false,
            toString: () => 'CacheItem',
            parseString: (key) => ({}),
            normalizeKey: (key) => key ?? {},
        },
        tenantKey: {
            isTenantDependent: true,
            isHardInvalidatable: false,
            toString: (key) => `${key.tenantId}`,
            parseString: (key) => {
                return { tenantId: parseInt(key) };
            },
            normalizeKey: (key) => {
                return populateKeyTenantIdIfMissing(key ?? {
                    tenantId: 0,
                });
            },
        },
        dateKey: {
            isTenantDependent: true,
            isHardInvalidatable: true,
            toString: (key) => `${key.tenantId}-${key.date}-${key.shift}`,
            parseString: (key) => {
                const parts = key.split('-');
                return {
                    tenantId: parseInt(parts[0]),
                    date: parts[1],
                    shift: parseInt(parts[2]),
                };
            },
            normalizeKey: (key) => {
                if (!key
                    || !key.date) {
                    throw new Error('key and key.date are required to use this cache');
                }
                populateKeyTenantIdIfMissing(key);
                return key;
            },
        },
    };
    function populateKeyTenantIdIfMissing(key) {
        if (!key.tenantId) {
            key.tenantId = listCacheHelpers.getSessionTenantId();
        }
        return key;
    }
    class ListCache {
        static lock = new AsyncLock();
        configuration;
        inMemoryCache = new Map();
        constructor(configuration) {
            this.configuration = configuration;
            listCacheHelpers.onSyncRequest(this.onSyncRequest.bind(this));
        }
        async getList(key) {
            key = this.normalizeKey(key);
            const cacheItem = await this.getOrCreateCacheItem(key);
            return cacheItem.items;
        }
        async getMap(key) {
            key = this.normalizeKey(key);
            const cacheItem = await this.getOrCreateCacheItem(key);
            return cacheItem.itemsMap;
        }
        async getOrCreateCacheItem(key) {
            const keyString = this.keyToString(key);
            // Only subscribe when a cache record is requested, not automatically in the constructor
            listCacheHelpers.subscribeToSyncRequests();
            // If the list for the given key has been already created in-memory, just return it as is
            let cacheItem = this.getInMemoryCacheItem(key);
            if (cacheItem) {
                return cacheItem;
            }
            const lockKey = `${this.name}:${keyString}`;
            return ListCache.lock.withLock(lockKey, async () => {
                // Check again if item appeared in memory while waiting for lock
                let cacheItem = this.getInMemoryCacheItem(key);
                if (cacheItem) {
                    return cacheItem;
                }
                // If we're creating a new item, the first do a cleanup for indexedDB:
                await this.cleanup(key);
                // Try to load from IndexedDB
                cacheItem = await this.getFromIndexedDB(key)
                    // Otherwise, create a new list item
                    ?? {
                        key: key,
                        items: [],
                        maxDateTime: null,
                        cacheCreationDateTime: null,
                        itemsMap: new Map(),
                        cacheName: this.name,
                        keyString: keyString,
                        cacheCodeVersion: CacheCodeVersion,
                        globalCacheVersion: this.getGlobalCacheVersion(),
                        isLastAccessed: true,
                        lastAccessedAt: moment().utc().format(),
                    };
                // Store in memory
                this.inMemoryCache.set(keyString, cacheItem);
                // Sync with backend
                await this.syncWithBackendInternal(cacheItem);
                return cacheItem;
            });
        }
        getInMemoryCacheItem(key) {
            const keyString = this.keyToString(key);
            if (this.inMemoryCache.has(keyString)) {
                const item = this.inMemoryCache.get(keyString);
                this.updateAccessTime(item);
                return item;
            }
            return null;
        }
        getStorageKey(key) {
            const keyString = this.keyToString(key);
            return [
                this.name,
                keyString,
                CacheCodeVersion,
                this.getGlobalCacheVersion(),
            ];
        }
        getStorageKeyForItem(item) {
            return [
                item.cacheName,
                item.keyString,
                item.cacheCodeVersion,
                item.globalCacheVersion,
            ];
        }
        keyToString(key) {
            return this.configuration.keyFormatter.toString(key);
        }
        updateAccessTime(cacheItem) {
            cacheItem.lastAccessedAt = moment().utc().format();
            cacheItem.isLastAccessed = true;
            // Update in IndexedDB asynchronously
            this.saveToIndexedDB(cacheItem).catch(err => console.error('Failed to update last accessed time in IndexedDB', err));
        }
        async saveToIndexedDB(cacheItem) {
            const db = await dbPromise;
            // Create a serializable version (Map can't be stored directly)
            const storableItem = {
                ...cacheItem,
                itemsMap: undefined,
            };
            try {
                await db.put(DB_STORES_CACHE_ITEM, storableItem);
            }
            catch (error) {
                console.error('Failed to save cache data to IndexedDB', error);
                // Try to recover by clearing potentially corrupted data
                try {
                    await db.delete(DB_STORES_CACHE_ITEM, this.getStorageKey(cacheItem.key));
                }
                catch (clearError) {
                    console.error('Failed to clean up corrupted cache entry', clearError);
                }
            }
        }
        async getFromIndexedDB(key) {
            try {
                const db = await dbPromise;
                const cacheKey = this.getStorageKey(key);
                const storedData = await db.get(DB_STORES_CACHE_ITEM, cacheKey);
                if (storedData) {
                    // Recreate the map since it wasn't serialized
                    storedData.itemsMap = new Map(storedData.items.map(x => [x.id, x]));
                    this.updateAccessTime(storedData);
                    return storedData;
                }
            }
            catch (error) {
                console.error(`Failed to retrieve cache data from IndexedDB`, error);
            }
            return null;
        }
        async cleanup(key) {
            const lockKey = `cleanup:${this.name}`;
            await ListCache.lock.withLock(lockKey, async () => {
                try {
                    const db = await dbPromise;
                    const tx = db.transaction(DB_STORES_CACHE_ITEM, 'readwrite');
                    const store = tx.objectStore(DB_STORES_CACHE_ITEM);
                    // Only get items for this specific cache name
                    const index = store.index('by-cache-name');
                    let cacheItems = await index.getAll(this.name);
                    // 1. First, remove records that have incompatible cacheCodeVersion, globalCacheVersion, or tenantId
                    const isInvalidKey = (item) => {
                        return item.cacheCodeVersion !== CacheCodeVersion
                            || item.globalCacheVersion !== this.getGlobalCacheVersion()
                            || (this.configuration.keyFormatter.isTenantDependent
                                && item.key.tenantId !== listCacheHelpers.getSessionTenantId());
                    };
                    for (const item of cacheItems) {
                        if (isInvalidKey(item)) {
                            const itemKey = this.getStorageKeyForItem(item);
                            await store.delete(itemKey);
                        }
                    }
                    cacheItems = cacheItems.filter(item => !isInvalidKey(item));
                    // 2. Then, (for caches with a matching cacheName), if there are any isLastAccessed caches, set their lastAccessedAt to now (and isLastAccessed to false unless its key matches the current key)
                    const lastAccessedItems = cacheItems.filter(item => item.isLastAccessed);
                    for (const item of lastAccessedItems) {
                        item.isLastAccessed = this.keyToString(item.key) === this.keyToString(key);
                        item.lastAccessedAt = moment().utc().format();
                        await store.put(item);
                    }
                    // 3. Then, for all items that have lastAccessedAt older than slidingExpirationTimeMinutes, remove them
                    const expirationMinutes = this.slidingExpirationTimeMinutes;
                    if (expirationMinutes > 0) {
                        const cutoffTime = moment().subtract(expirationMinutes, 'minutes').utc().format();
                        // Filter expired items by comparing lastAccessedAt
                        const expiredItems = cacheItems.filter(item => item.lastAccessedAt && item.lastAccessedAt < cutoffTime);
                        for (const item of expiredItems) {
                            const itemKey = this.getStorageKeyForItem(item);
                            await store.delete(itemKey);
                        }
                    }
                    await tx.done;
                }
                catch (error) {
                    console.error('Failed to clean up caches', error);
                }
            });
        }
        async syncWithBackend(key) {
            key = this.normalizeKey(key);
            const keyString = this.keyToString(key);
            const cacheItem = this.getInMemoryCacheItem(key);
            if (!cacheItem) {
                console.warn(`Cache item for key ${keyString} does not exist in memory. Cannot sync from backend.`);
                return;
            }
            const lockKey = `${this.name}:${keyString}`;
            await ListCache.lock.withLock(lockKey, async () => {
                await this.syncWithBackendInternal(cacheItem);
            });
        }
        async syncWithBackendInternal(cacheItem) {
            const request = {
                key: cacheItem.key,
                afterDateTime: cacheItem.maxDateTime,
                cacheCreationDateTime: this.configuration.keyFormatter.isHardInvalidatable ? cacheItem.cacheCreationDateTime : null,
            };
            try {
                const backendData = await this.getBackendCacheData(request);
                // Process backend data (whether it's initial or incremental)
                backendData.items.forEach(newItem => {
                    const existingItem = cacheItem.itemsMap.get(newItem.id);
                    if (existingItem) {
                        // Note that the item is not guaranteed to have actually been changed at this point. For outgoing events, compare the actual data first.
                        // Update existing item properties
                        Object.assign(existingItem, newItem);
                    }
                    else {
                        // Add to items array and map
                        cacheItem.items.push(newItem);
                        cacheItem.itemsMap.set(newItem.id, newItem);
                    }
                });
                // Remove deleted items
                for (let i = cacheItem.items.length - 1; i >= 0; i--) {
                    if (cacheItem.items[i].isDeleted
                        || backendData.hardInvalidate && !backendData.items.some(b => b.id == cacheItem.items[i].id)) {
                        if (backendData.hardInvalidate) {
                            cacheItem.items[i].isDeleted = true;
                        }
                        const id = cacheItem.items[i].id;
                        cacheItem.items.splice(i, 1);
                        cacheItem.itemsMap.delete(id);
                    }
                }
                cacheItem.maxDateTime = backendData.maxDateTime;
                cacheItem.cacheCreationDateTime = backendData.cacheCreationDateTime;
                cacheItem.lastAccessedAt = moment().utc().format();
                cacheItem.isLastAccessed = true;
                await this.saveToIndexedDB(cacheItem);
            }
            catch (error) {
                console.error(`Failed to fetch backend cache data for ${this.name}`, error);
            }
        }
        async onSyncRequest(syncRequest) {
            // todo remove console.log calls after qa4 testing is complete
            if (syncRequest.cacheName !== this.name) {
                return;
            }
            const keyString = this.keyToString(syncRequest.key);
            console.log(`Received sync request for cache ${this.name} with key ${keyString}`);
            let cacheItem = this.getInMemoryCacheItem(syncRequest.key);
            if (!cacheItem) {
                console.log(`Cache item for key ${keyString} does not exist in memory. Skipping.`);
                return;
            }
            const lockKey = `${this.name}:${keyString}`;
            await ListCache.lock.withLock(lockKey, async () => {
                await this.syncWithBackendInternal(cacheItem);
            });
            console.log(`Sync completed for cache ${this.name} with key ${keyString}`);
        }
        getSelectListMethod(config) {
            config ??= {};
            if (!this.isEnabled && config.fallbackMethod) {
                return config.fallbackMethod;
            }
            const key = this.normalizeKey(config.key);
            const nameField = config.nameField || 'name';
            const resultMethod = async (input) => {
                const term = (input.term || '').toLowerCase();
                let items = await this.getList(key);
                if (config.fallbackMethod
                    && config.fallbackChooser?.(input)) {
                    console.warn('Provided select2 input does not support caching, using fallback method', input);
                    await new Promise(resolve => setTimeout(resolve, 250)); // Restore the delay - although it's still not through UI. If we get this warning, we should adjust the logic to avoid using the cached select list or update cache to support the new parameter
                    return await config.fallbackMethod(input);
                }
                if (config.filter) {
                    let filterAsyncState = undefined;
                    if (config.filterAsyncState) {
                        filterAsyncState = await config.filterAsyncState(input);
                    }
                    items = items.filter((item) => config.filter(input, item, filterAsyncState));
                }
                const idField = config.idFieldGetter?.(input)
                    ?? config.idField
                    ?? 'id';
                const result = items.map(item => ({
                    id: item[idField]?.toString(),
                    name: item[nameField],
                    item: config.modelCallback?.(item),
                }));
                this.sortByName(result);
                if (!term) {
                    return {
                        totalCount: items.length,
                        items: result,
                    };
                }
                const startsWith = [];
                const contains = [];
                for (const item of result) {
                    const name = item.name.toLowerCase();
                    if (name.startsWith(term)) {
                        startsWith.push(item);
                    }
                    else if (name.includes(term)) {
                        contains.push(item);
                    }
                }
                const filteredResult = [
                    ...startsWith,
                    ...contains,
                ];
                return {
                    totalCount: filteredResult.length,
                    items: filteredResult,
                };
            };
            resultMethod.isCached = true;
            return resultMethod;
        }
        sortByName(arr) {
            arr.sort((a, b) => a.name.localeCompare(b.name, undefined, {
                sensitivity: 'base',
            }));
        }
        get name() {
            return this.configuration.name;
        }
        get isEnabled() {
            return listCacheHelpers.setting.getBoolean(`App.ListCaches.${this.name}.Frontend.IsEnabled`);
        }
        get slidingExpirationTimeMinutes() {
            return listCacheHelpers.setting.getInt(`App.ListCaches.${this.name}.Frontend.SlidingExpirationTimeMinutes`);
        }
        getBackendCacheData(request) {
            request.key = this.normalizeKey(request.key);
            return listCacheHelpers.getBackendCacheData(this.name, request);
        }
        normalizeKey(key) {
            return this.configuration.keyFormatter.normalizeKey(key);
        }
        getGlobalCacheVersion() {
            return listCacheHelpers.setting.getInt('App.ListCaches.GlobalCacheVersion');
        }
    }
    var listCacheHelpers = {
        setting: {
            getBoolean: function (key) {
                return window.abp.setting.getBoolean(key);
            },
            getInt: function (key) {
                return window.abp.setting.getInt(key);
            },
            get: function (key) {
                return window.abp.setting.get(key);
            },
        },
        getBackendCacheData: function (cacheName, request) {
            return (window.abp.services.app.caching[listCacheHelpers.toCamelCase(cacheName)])(request);
        },
        toCamelCase: function (value) {
            return window.abp.utils.toCamelCase(value);
        },
        getSessionTenantId: function () {
            let tenantId = window.abp.session.tenantId;
            if (!tenantId) {
                throw new Error('TenantId is requred for getSessionTenantId');
            }
            return tenantId;
        },
        subscribeToSyncRequests: function () {
            window.abp.signalr.subscribeToListCacheSyncRequests();
        },
        onSyncRequest: function (callback) {
            window.abp.event.on('abp.signalR.receivedListCacheSyncRequest', callback);
        },
    };
    window.ListCache = ListCache;
    window.listCache = {
        order: new ListCache({
            name: 'OrderListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.order,
        }),
        orderLine: new ListCache({
            name: 'OrderLineListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.orderLine,
        }),
        orderLineTruck: new ListCache({
            name: 'OrderLineTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.orderLineTruck,
        }),
        driverAssignment: new ListCache({
            name: 'DriverAssignmentListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.driverAssignment,
        }),
        driver: new ListCache({
            name: 'DriverListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.driver,
        }),
        leaseHaulerDriver: new ListCache({
            name: 'LeaseHaulerDriverListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.leaseHaulerDriver,
        }),
        truck: new ListCache({
            name: 'TruckListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.truck,
        }),
        leaseHaulerTruck: new ListCache({
            name: 'LeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.leaseHaulerTruck,
        }),
        availableLeaseHaulerTruck: new ListCache({
            name: 'AvailableLeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.availableLeaseHaulerTruck,
        }),
        leaseHauler: new ListCache({
            name: 'LeaseHaulerListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.leaseHauler,
        }),
        insurance: new ListCache({
            name: 'InsuranceListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.insurance,
        }),
        user: new ListCache({
            name: 'UserListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.user,
        }),
        leaseHaulerUser: new ListCache({
            name: 'LeaseHaulerUserListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.leaseHaulerUser,
        }),
        customer: new ListCache({
            name: 'CustomerListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.customer,
        }),
        customerContact: new ListCache({
            name: 'CustomerContactListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.customerContact,
        }),
        location: new ListCache({
            name: 'LocationListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.location,
        }),
        item: new ListCache({
            name: 'ItemListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.item,
        }),
        vehicleCategory: new ListCache({
            name: 'VehicleCategoryListCache',
            keyFormatter: listKeyFormatters.emptyKey,
            entityType: window.abp.enums.entityEnum.vehicleCategory,
        }),
        unitOfMeasure: new ListCache({
            name: 'UnitOfMeasureListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: window.abp.enums.entityEnum.unitOfMeasure,
        }),
        orderLineVehicleCategory: new ListCache({
            name: 'OrderLineVehicleCategoryListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.orderLineVehicleCategory,
        }),
        leaseHaulerRequest: new ListCache({
            name: 'LeaseHaulerRequestListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.leaseHaulerRequest,
        }),
        requestedLeaseHaulerTruck: new ListCache({
            name: 'RequestedLeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.requestedLeaseHaulerTruck,
        }),
        dispatch: new ListCache({
            name: 'DispatchListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.dispatch,
        }),
        load: new ListCache({
            name: 'LoadListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.load,
        }),
        ticket: new ListCache({
            name: 'TicketListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: window.abp.enums.entityEnum.ticket,
        }),
        fuelSurchargeCalculation: new ListCache({
            name: 'FuelSurchargeCalculationListCache',
            keyFormatter: listKeyFormatters.tenantKey,
        }),
        office: new ListCache({
            name: 'OfficeListCache',
            keyFormatter: listKeyFormatters.tenantKey,
        }),
        taxRate: new ListCache({
            name: 'TaxRateListCache',
            keyFormatter: listKeyFormatters.tenantKey,
        }),
    };
})();
//(async function () {
//    let listCache = (window as any).listCache;
//    //sample usage:
//    let drivers = await listCache.driver.getList();
//})();
