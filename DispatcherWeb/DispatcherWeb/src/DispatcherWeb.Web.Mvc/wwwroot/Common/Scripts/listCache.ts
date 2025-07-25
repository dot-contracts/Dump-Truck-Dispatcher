/// <reference path="../../../node_modules/@dumptruckdispatcher/datatables-helper/typings/moment.d.ts" />
/// <reference path="../../lib/idb/entry.d.ts" />

(function () {
    // Import idb functionality
    const { openDB, deleteDB }: {
        openDB<DBTypes extends DBSchema | unknown = unknown>(name: string, version?: number, { blocked, upgrade, blocking, terminated }?: OpenDBCallbacks<DBTypes>): Promise<IDBPDatabase<DBTypes>>;
        deleteDB(name: string, { blocked }?: DeleteDBCallbacks): Promise<void>;
    } = (window as any).idb;

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
        private locks: Map<string, Promise<any>> = new Map();

        async withLock<T>(key: string, operation: () => Promise<T>): Promise<T> {
            // Check if there's already an active lock
            const existingLock = this.locks.get(key);
            if (existingLock) {
                // Wait for the existing operation to complete
                await existingLock;

                // After waiting, try again (recursive) - this ensures proper queuing
                return this.withLock(key, operation);
            }

            // Create a new lock promise
            let resolveLock!: Function;
            const lockPromise = new Promise(resolve => {
                resolveLock = resolve;
            });

            // Register the lock
            this.locks.set(key, lockPromise);

            try {
                // Execute the operation
                const result = await operation();
                return result;
            } finally {
                // Release the lock
                resolveLock();
                this.locks.delete(key);
            }
        }

        isLocked(key: string): boolean {
            return this.locks.has(key);
        }
    }

    interface IListCacheSyncRequest<TListKey> {
        cacheName: string;
        key: TListKey;
    }

    interface IAuditableCacheItem {
        id: number;
        isDeleted: boolean;
        deletionTime: string;
        creationTime: string;
        lastModificationTime: string;
        [field: string]: any;
    }

    interface IListCacheItem<TListKey> {
        key: TListKey;
        items: IAuditableCacheItem[];
        maxDateTime: string | null;
        cacheCreationDateTime: string | null;
    }

    interface IRemoteListCacheItem<TListKey> extends IListCacheItem<TListKey> {
        hardInvalidate: boolean;
    }

    interface ILocalListCacheItem<TListKey> extends IListCacheItem<TListKey> {
        itemsMap?: Map<number, IAuditableCacheItem>; // Not serialized, recreated after deserialization
        // Below are the additional key parts that we'd need for a unique key, and they're separate properties for easier cache invalidation
        cacheName: string;
        keyString: string;
        cacheCodeVersion: number;
        globalCacheVersion: number;
        //
        isLastAccessed: boolean;
        lastAccessedAt: string; // ISO formatted UTC datetime, used to determine if the cache is expired
    }

    interface IGetListCacheDataRequest<TListKey> {
        key: TListKey;
        afterDateTime: string | null;
        cacheCreationDateTime: string | null;
    }

    interface IListCacheKey {
    }

    interface IListCacheEmptyKey extends IListCacheKey {
    }

    interface IListCacheTenantKey extends IListCacheKey {
        tenantId: number;
    }

    interface IListCacheDateKey extends IListCacheTenantKey {
        date: string; // yyyy-MM-dd date string, e.g. "2023-10-01"
        shift: number; // shiftEnum
    }

    interface IListCacheFormatter<TListCacheKey> {
        isTenantDependent: boolean;
        isHardInvalidatable: boolean;
        toString(key: TListCacheKey): string;
        parseString(key: string): TListCacheKey;
        normalizeKey(key?: TListCacheKey): TListCacheKey;
    }

    interface ListKeyFormatters {
        emptyKey: IListCacheFormatter<IListCacheEmptyKey>,
        tenantKey: IListCacheFormatter<IListCacheTenantKey>,
        dateKey: IListCacheFormatter<IListCacheDateKey>,
    }

    let listKeyFormatters: ListKeyFormatters = {
        emptyKey: {
            isTenantDependent: false,
            isHardInvalidatable: false,
            toString: () => 'CacheItem',
            parseString: (key: string) => ({}),
            normalizeKey: (key) => key ?? {},
        },
        tenantKey: {
            isTenantDependent: true,
            isHardInvalidatable: false,
            toString: (key: IListCacheTenantKey) => `${key.tenantId}`,
            parseString: (key: string) => {
                return { tenantId: parseInt(key) };
            },
            normalizeKey: (key: IListCacheTenantKey) => {
                return populateKeyTenantIdIfMissing(key ?? {
                    tenantId: 0,
                });
            },
        },
        dateKey: {
            isTenantDependent: true,
            isHardInvalidatable: true,
            toString: (key: IListCacheDateKey) => `${key.tenantId}-${key.date}-${key.shift}`,
            parseString: (key: string) => {
                const parts = key.split('-');
                return {
                    tenantId: parseInt(parts[0]),
                    date: parts[1],
                    shift: parseInt(parts[2]),
                };
            },
            normalizeKey: (key: IListCacheDateKey) => {
                if (!key
                    || !key.date
                ) {
                    throw new Error('key and key.date are required to use this cache');
                }
                populateKeyTenantIdIfMissing(key);
                return key;
            },
        },
    };

    function populateKeyTenantIdIfMissing(key: IListCacheTenantKey) {
        if (!key.tenantId) {
            key.tenantId = listCacheHelpers.getSessionTenantId();
        }

        return key;
    }

    interface IListCacheConfiguration<TListKey> {
        name: string;
        keyFormatter: IListCacheFormatter<TListKey>;
        entityType?: number; //deprecated
    }

    class ListCache<TListKey> {
        private static lock = new AsyncLock();
        private configuration: IListCacheConfiguration<TListKey>;
        private inMemoryCache: Map<string, ILocalListCacheItem<TListKey>> = new Map();

        constructor(configuration: IListCacheConfiguration<TListKey>) {
            this.configuration = configuration;
            listCacheHelpers.onSyncRequest<TListKey>(this.onSyncRequest.bind(this));
        }

        public async getList(key: TListKey): Promise<IAuditableCacheItem[]> {
            key = this.normalizeKey(key);

            const cacheItem = await this.getOrCreateCacheItem(key);
            return cacheItem.items;
        }

        public async getMap(key: TListKey): Promise<Map<number, IAuditableCacheItem>> {
            key = this.normalizeKey(key);

            const cacheItem = await this.getOrCreateCacheItem(key);
            return cacheItem.itemsMap!;
        }

        private async getOrCreateCacheItem(key: TListKey): Promise<ILocalListCacheItem<TListKey>> {
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
                    itemsMap: new Map<number, IAuditableCacheItem>(),
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

        private getInMemoryCacheItem(key: TListKey): ILocalListCacheItem<TListKey> | null {
            const keyString = this.keyToString(key);
            if (this.inMemoryCache.has(keyString)) {
                const item = this.inMemoryCache.get(keyString)!;

                this.updateAccessTime(item);

                return item;
            }

            return null;
        }

        private getStorageKey(key: TListKey): [string, string, number, number] {
            const keyString = this.keyToString(key);
            return [
                this.name,
                keyString,
                CacheCodeVersion,
                this.getGlobalCacheVersion(),
            ];
        }

        private getStorageKeyForItem(item: ILocalListCacheItem<TListKey>): [string, string, number, number] {
            return [
                item.cacheName,
                item.keyString,
                item.cacheCodeVersion,
                item.globalCacheVersion,
            ];
        }

        private keyToString(key: TListKey): string {
            return this.configuration.keyFormatter.toString(key);
        }

        private updateAccessTime(cacheItem: ILocalListCacheItem<TListKey>): void {
            cacheItem.lastAccessedAt = moment().utc().format();
            cacheItem.isLastAccessed = true;

            // Update in IndexedDB asynchronously
            this.saveToIndexedDB(cacheItem).catch(err =>
                console.error('Failed to update last accessed time in IndexedDB', err));
        }

        private async saveToIndexedDB(cacheItem: ILocalListCacheItem<TListKey>): Promise<void> {
            const db = await dbPromise;

            // Create a serializable version (Map can't be stored directly)
            const storableItem = {
                ...cacheItem,
                itemsMap: undefined,
            };

            try {
                await db.put(DB_STORES_CACHE_ITEM, storableItem);
            } catch (error) {
                console.error('Failed to save cache data to IndexedDB', error);
                // Try to recover by clearing potentially corrupted data
                try {
                    await db.delete(DB_STORES_CACHE_ITEM, this.getStorageKey(cacheItem.key));
                } catch (clearError) {
                    console.error('Failed to clean up corrupted cache entry', clearError);
                }
            }
        }

        private async getFromIndexedDB(key: TListKey): Promise<ILocalListCacheItem<TListKey> | null> {
            try {
                const db = await dbPromise;
                const cacheKey = this.getStorageKey(key);

                const storedData = await db.get(DB_STORES_CACHE_ITEM, cacheKey) as ILocalListCacheItem<TListKey> | null;

                if (storedData) {
                    // Recreate the map since it wasn't serialized
                    storedData.itemsMap = new Map<number, IAuditableCacheItem>(
                        storedData.items.map(x => [x.id, x])
                    );

                    this.updateAccessTime(storedData);

                    return storedData;
                }
            } catch (error) {
                console.error(`Failed to retrieve cache data from IndexedDB`, error);
            }

            return null;
        }

        private async cleanup(key: TListKey): Promise<void> {
            const lockKey = `cleanup:${this.name}`;
            await ListCache.lock.withLock(lockKey, async () => {
                try {
                    const db = await dbPromise;
                    const tx = db.transaction(DB_STORES_CACHE_ITEM, 'readwrite');
                    const store = tx.objectStore(DB_STORES_CACHE_ITEM);

                    // Only get items for this specific cache name
                    const index = store.index('by-cache-name');
                    let cacheItems = await index.getAll(this.name) as ILocalListCacheItem<TListKey>[];

                    // 1. First, remove records that have incompatible cacheCodeVersion, globalCacheVersion, or tenantId
                    const isInvalidKey = (item: ILocalListCacheItem<TListKey>) => {
                        return item.cacheCodeVersion !== CacheCodeVersion
                            || item.globalCacheVersion !== this.getGlobalCacheVersion()
                            || (this.configuration.keyFormatter.isTenantDependent
                                && (item.key as IListCacheTenantKey).tenantId !== listCacheHelpers.getSessionTenantId()
                            );
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
                } catch (error) {
                    console.error('Failed to clean up caches', error);
                }
            });
        }

        public async syncWithBackend(key: TListKey): Promise<void> {
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

        private async syncWithBackendInternal(cacheItem: ILocalListCacheItem<TListKey>): Promise<void> {
            const request: IGetListCacheDataRequest<TListKey> = {
                key: cacheItem.key,
                afterDateTime: cacheItem.maxDateTime,
                cacheCreationDateTime: this.configuration.keyFormatter.isHardInvalidatable ? cacheItem.cacheCreationDateTime : null,
            };

            try {
                const backendData = await this.getBackendCacheData(request);

                // Process backend data (whether it's initial or incremental)
                backendData.items.forEach(newItem => {
                    const existingItem = cacheItem.itemsMap!.get(newItem.id);
                    if (existingItem) {
                        // Note that the item is not guaranteed to have actually been changed at this point. For outgoing events, compare the actual data first.
                        // Update existing item properties
                        Object.assign(existingItem, newItem);
                    } else {
                        // Add to items array and map
                        cacheItem.items.push(newItem);
                        cacheItem.itemsMap!.set(newItem.id, newItem);
                    }
                });

                // Remove deleted items
                for (let i = cacheItem.items.length - 1; i >= 0; i--) {
                    if (cacheItem.items[i].isDeleted
                        || backendData.hardInvalidate && !backendData.items.some(b => b.id == cacheItem.items[i].id)
                    ) {
                        if (backendData.hardInvalidate) {
                            cacheItem.items[i].isDeleted = true;
                        }
                        const id = cacheItem.items[i].id;
                        cacheItem.items.splice(i, 1);
                        cacheItem.itemsMap!.delete(id);
                    }
                }

                cacheItem.maxDateTime = backendData.maxDateTime;
                cacheItem.cacheCreationDateTime = backendData.cacheCreationDateTime;
                cacheItem.lastAccessedAt = moment().utc().format();
                cacheItem.isLastAccessed = true;

                await this.saveToIndexedDB(cacheItem);
            } catch (error) {
                console.error(`Failed to fetch backend cache data for ${this.name}`, error);
            }
        }

        private async onSyncRequest(syncRequest: IListCacheSyncRequest<TListKey>): Promise<void> {
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

        public getSelectListMethod(config: ISelectListMethodConfiguration<TListKey>): SelectListMethod {
            config ??= {};
            if (!this.isEnabled && config.fallbackMethod) {
                return config.fallbackMethod;
            }

            const key: TListKey = this.normalizeKey(config.key);
            const nameField = config.nameField || 'name';

            const resultMethod = async (input: ISelectListMethodArgs): Promise<ISelectListMethodResult> => {
                const term = (input.term || '').toLowerCase();
                let items = await this.getList(key);

                if (config.fallbackMethod
                    && config.fallbackChooser?.(input)
                ) {
                    console.warn('Provided select2 input does not support caching, using fallback method', input);
                    await new Promise(resolve => setTimeout(resolve, 250)); // Restore the delay - although it's still not through UI. If we get this warning, we should adjust the logic to avoid using the cached select list or update cache to support the new parameter
                    return await config.fallbackMethod(input);
                }

                if (config.filter) {
                    let filterAsyncState = undefined;
                    if (config.filterAsyncState) {
                        filterAsyncState = await config.filterAsyncState(input);
                    }

                    items = items.filter((item) => config.filter!(input, item, filterAsyncState));
                }

                const idField = config.idFieldGetter?.(input)
                    ?? config.idField
                    ?? 'id';

                const result: ISelectListMethodResultItem[] = items.map(item => ({
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

                const startsWith: ISelectListMethodResultItem[] = [];
                const contains: ISelectListMethodResultItem[] = [];

                for (const item of result) {
                    const name = item.name.toLowerCase();
                    if (name.startsWith(term)) {
                        startsWith.push(item);
                    } else if (name.includes(term)) {
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

        private sortByName(arr: { name: string; }[]): void {
            arr.sort((a, b) =>
                a.name.localeCompare(
                    b.name,
                    undefined,
                    {
                        sensitivity: 'base',
                    },
                ),
            );
        }

        get name(): string {
            return this.configuration.name;
        }

        get isEnabled(): boolean {
            return listCacheHelpers.setting.getBoolean(`App.ListCaches.${this.name}.Frontend.IsEnabled`);
        }

        get slidingExpirationTimeMinutes(): number {
            return listCacheHelpers.setting.getInt(`App.ListCaches.${this.name}.Frontend.SlidingExpirationTimeMinutes`);
        }

        public getBackendCacheData(request: IGetListCacheDataRequest<TListKey>) {
            request.key = this.normalizeKey(request.key);
            return listCacheHelpers.getBackendCacheData<TListKey>(this.name, request);
        }

        public normalizeKey(key?: TListKey): TListKey {
            return this.configuration.keyFormatter.normalizeKey(key);
        }

        getGlobalCacheVersion(): number {
            return listCacheHelpers.setting.getInt('App.ListCaches.GlobalCacheVersion');
        }

    }

    interface ISelectListMethodArgs {
        term: string;
    }
    interface ISelectListMethodResult {
        totalCount: number;
        items: ISelectListMethodResultItem[];
    }
    interface ISelectListMethodResultItem {
        id: string;
        name: string;
        item?: object; // Additional properties can be added as needed
    }

    type SelectListMethod = (args: ISelectListMethodArgs) => Promise<ISelectListMethodResult>;
    interface ISelectListMethodConfiguration<TListKey> {
        key?: TListKey; // Semi-optional key to use for the cache. If not provided, the method will try to use key normalizer or throw
        fallbackMethod?: SelectListMethod;
        fallbackChooser?: (input: ISelectListMethodArgs) => boolean; // Return true to force the use of fallbackMethod
        modelCallback?: (auditableCacheItem: IAuditableCacheItem) => object; //to populate the item property in the response
        idField?: string;
        idFieldGetter?: (input: ISelectListMethodArgs) => string; // If provided, takes precedence over idField
        nameField?: string;
        filterAsyncState?: (input: ISelectListMethodArgs) => Promise<object>;
        filter?: (input: ISelectListMethodArgs, item: IAuditableCacheItem, filterAsyncState?: object) => boolean;
    }

    var listCacheHelpers = {
        setting: {
            getBoolean: function (key: string): boolean {
                return (window as any).abp.setting.getBoolean(key);
            },
            getInt: function (key: string): number {
                return (window as any).abp.setting.getInt(key);
            },
            get: function (key: string): string | undefined {
                return (window as any).abp.setting.get(key);
            },
        },
        getBackendCacheData: function <TListKey>(cacheName: string, request: IGetListCacheDataRequest<TListKey>): Promise<IRemoteListCacheItem<TListKey>> {
            return ((window as any).abp.services.app.caching[listCacheHelpers.toCamelCase(cacheName)])(request);
        },
        toCamelCase: function (value: string): string {
            return (window as any).abp.utils.toCamelCase(value);
        },
        getSessionTenantId: function (): number {
            let tenantId = (window as any).abp.session.tenantId as number | null;
            if (!tenantId) {
                throw new Error('TenantId is requred for getSessionTenantId');
            }
            return tenantId;
        },
        subscribeToSyncRequests: function () {
            (window as any).abp.signalr.subscribeToListCacheSyncRequests();
        },
        onSyncRequest: function <TListKey>(callback: (syncRequest: IListCacheSyncRequest<TListKey>) => void) {
            (window as any).abp.event.on('abp.signalR.receivedListCacheSyncRequest', callback);
        },
    };

    (window as any).ListCache = ListCache;
    (window as any).listCache = {
        order: new ListCache<IListCacheDateKey>({
            name: 'OrderListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.order,
        }),
        orderLine: new ListCache<IListCacheDateKey>({
            name: 'OrderLineListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.orderLine,
        }),
        orderLineTruck: new ListCache<IListCacheDateKey>({
            name: 'OrderLineTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.orderLineTruck,
        }),
        driverAssignment: new ListCache<IListCacheDateKey>({
            name: 'DriverAssignmentListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.driverAssignment,
        }),
        driver: new ListCache<IListCacheTenantKey>({
            name: 'DriverListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.driver,
        }),
        leaseHaulerDriver: new ListCache<IListCacheTenantKey>({
            name: 'LeaseHaulerDriverListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.leaseHaulerDriver,
        }),
        truck: new ListCache<IListCacheTenantKey>({
            name: 'TruckListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.truck,
        }),
        leaseHaulerTruck: new ListCache<IListCacheTenantKey>({
            name: 'LeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.leaseHaulerTruck,
        }),
        availableLeaseHaulerTruck: new ListCache<IListCacheDateKey>({
            name: 'AvailableLeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.availableLeaseHaulerTruck,
        }),
        leaseHauler: new ListCache<IListCacheTenantKey>({
            name: 'LeaseHaulerListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.leaseHauler,
        }),
        insurance: new ListCache<IListCacheTenantKey>({
            name: 'InsuranceListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.insurance,
        }),
        user: new ListCache<IListCacheTenantKey>({
            name: 'UserListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.user,
        }),
        leaseHaulerUser: new ListCache<IListCacheTenantKey>({
            name: 'LeaseHaulerUserListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.leaseHaulerUser,
        }),
        customer: new ListCache<IListCacheTenantKey>({
            name: 'CustomerListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.customer,
        }),
        customerContact: new ListCache<IListCacheTenantKey>({
            name: 'CustomerContactListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.customerContact,
        }),
        location: new ListCache<IListCacheTenantKey>({
            name: 'LocationListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.location,
        }),
        item: new ListCache<IListCacheTenantKey>({
            name: 'ItemListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.item,
        }),
        vehicleCategory: new ListCache<IListCacheEmptyKey>({
            name: 'VehicleCategoryListCache',
            keyFormatter: listKeyFormatters.emptyKey,
            entityType: (window as any).abp.enums.entityEnum.vehicleCategory,
        }),
        unitOfMeasure: new ListCache<IListCacheTenantKey>({
            name: 'UnitOfMeasureListCache',
            keyFormatter: listKeyFormatters.tenantKey,
            entityType: (window as any).abp.enums.entityEnum.unitOfMeasure,
        }),
        orderLineVehicleCategory: new ListCache<IListCacheDateKey>({
            name: 'OrderLineVehicleCategoryListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.orderLineVehicleCategory,
        }),
        leaseHaulerRequest: new ListCache<IListCacheDateKey>({
            name: 'LeaseHaulerRequestListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.leaseHaulerRequest,
        }),
        requestedLeaseHaulerTruck: new ListCache<IListCacheDateKey>({
            name: 'RequestedLeaseHaulerTruckListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.requestedLeaseHaulerTruck,
        }),
        dispatch: new ListCache<IListCacheDateKey>({
            name: 'DispatchListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.dispatch,
        }),
        load: new ListCache<IListCacheDateKey>({
            name: 'LoadListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.load,
        }),
        ticket: new ListCache<IListCacheDateKey>({
            name: 'TicketListCache',
            keyFormatter: listKeyFormatters.dateKey,
            entityType: (window as any).abp.enums.entityEnum.ticket,
        }),
        fuelSurchargeCalculation: new ListCache<IListCacheTenantKey>({
            name: 'FuelSurchargeCalculationListCache',
            keyFormatter: listKeyFormatters.tenantKey,
        }),
        office: new ListCache<IListCacheTenantKey>({
            name: 'OfficeListCache',
            keyFormatter: listKeyFormatters.tenantKey,
        }),
        taxRate: new ListCache<IListCacheTenantKey>({
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
