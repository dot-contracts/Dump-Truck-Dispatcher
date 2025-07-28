using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.Timing;
using Castle.Core.Logging;
using DispatcherWeb.Caching.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Caching
{
    #region Simpler constructor overloads

    /// <summary>
    /// Suitable for entities where id is int
    /// </summary>
    public abstract class ListCacheBase<TListKey, TItem, TEntity>
        : ListCacheBase<TListKey, TItem, int, TEntity>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<int>
        where TEntity : IDeletionAudited, ICreationAudited, IModificationAudited
    {
        protected ListCacheBase(ListCacheBaseDependency listCacheBaseDependency)
            : base(listCacheBaseDependency)
        {
        }
    }

    public abstract class ListCacheBase<TListKey, TItem, TItemKey, TEntity>
        : ListCacheBase<ListCacheItem<TListKey, TItem, TItemKey>, TListKey, TItem, TItemKey, TEntity>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<TItemKey>
        where TItemKey : struct, IEquatable<TItemKey>
        where TEntity : IDeletionAudited, ICreationAudited, IModificationAudited
    {
        protected ListCacheBase(ListCacheBaseDependency listCacheBaseDependency)
            : base(listCacheBaseDependency)
        {
        }
    }

    #endregion

    public abstract class ListCacheBase<TListCacheItem, TListKey, TItem, TItemKey, TEntity>
        : IListCache<TListKey, TItem, TItemKey>,
        IAsyncEventHandler<EntityUpdatedEventData<TEntity>>,
        IAsyncEventHandler<EntityCreatedEventData<TEntity>>,
        IAsyncEventHandler<EntityDeletedEventData<TEntity>>
        where TListCacheItem : ListCacheItem<TListKey, TItem, TItemKey>, new()
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<TItemKey>
        where TItemKey : struct, IEquatable<TItemKey>
        where TEntity : IDeletionAudited, ICreationAudited, IModificationAudited
    {
        private readonly ConcurrentDictionary<string, Timer> _syncRequestDebounceTimers = new();
        protected readonly ICacheManager CacheManager;
        protected readonly ISettingManager SettingManager;
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        protected readonly ListCacheDateKeyLookupService DateKeyLookup;
        protected readonly ISignalRCommunicator SignalRCommunicator;
        public ILogger Logger { get; set; }
        public abstract string CacheName { get; }
        public const string StaleStateCacheSuffix = ".StaleState";

        public ListCacheBase(
            ListCacheBaseDependency listCacheBaseDependency
        )
        {
            CacheManager = listCacheBaseDependency.CacheManager;
            SettingManager = listCacheBaseDependency.SettingManager;
            UnitOfWorkManager = listCacheBaseDependency.UnitOfWorkManager;
            DateKeyLookup = listCacheBaseDependency.DateKeyLookup;
            SignalRCommunicator = listCacheBaseDependency.SignalRCommunicator;
            Logger = NullLogger.Instance;

            if (CacheManager is RedisInvalidatableInMemoryCacheManager inMemoryCacheManager)
            {
                inMemoryCacheManager.RegisterListCache(this);
            }
        }



        #region GetList

        public async Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetListOrThrow(TListKey key)
        {
            if (!await IsEnabled())
            {
                throw new ApplicationException("The cache is disabled, you need to check that all the required caches are enabled before using this route");
            }

            return await GetList(new GetListCacheListInput<TListKey>(key));
        }

        public async Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetList(TListKey key)
        {
            return await GetList(new GetListCacheListInput<TListKey>(key));
        }

        public async Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetList(GetListCacheListInput<TListKey> input)
        {
            var key = input.Key;
            if (!await IsEnabled())
            {
                var hardInvalidate = input.ValidateCacheCreationDateTime(null);
                var dbItems = await GetAllItemsFromDbWithUow(key, input.AfterDateTime);
                return new ListCacheItemDto<TListKey, TItem, TItemKey>
                {
                    Key = key,
                    Items = dbItems,
                    MaxDateTime = GetMaxDateTime(dbItems),
                    HardInvalidate = hardInvalidate,
                };
            }

            var staleState = await GetStaleState(key);
            var cache = await GetCache();

            var cacheItem = await cache.GetAsync(key.ToStringKey(), async _ =>
            {
                var items = await GetAllItemsFromDbWithUow(key);

                staleState = await MarkAsNotStale(staleState);

                return new TListCacheItem
                {
                    Key = key,
                    Items = items,
                };
            });

            if (!staleState.IsStale)
            {
                return ConvertToListDto(cacheItem, input);
            }

            cacheItem = await cache.WithLockAsync(key.ToStringKey(), async () =>
            {
                var suppressSave = false;

                staleState = await GetStaleState(key);
                if (!staleState.IsStale)
                {
                    // the cache was updated by another thread while we were waiting for the lock, we can return the updated value now
                    var cacheItemOrDefault = await cache.TryGetValueAsync(key.ToStringKey());
                    if (cacheItemOrDefault.HasValue)
                    {
                        return cacheItemOrDefault.Value;
                    }

                    //on the off-chance that we don't have the value in the cache anymore, we can try to update the values that we've received on the previous step rather than returning stale data or throwing and exception, but we should log this to see how often (if ever) this happens
                    Logger.Warn($"Cache {CacheName} for key {key} was unmarked as stale by another thread, but the value was not found in the cache. Falling back to updating the local variable data from db");
                    suppressSave = true;
                }

                var maxDateTime = GetMaxDateTime(cacheItem.Items);

                var updatedItems = await GetAllItemsFromDbWithUow(key, maxDateTime);


                foreach (var updatedItem in updatedItems)
                {
                    cacheItem.Items.RemoveAll(x => x.Id.Equals(updatedItem.Id));
                    cacheItem.Items.Add(updatedItem);
                }

                if (!suppressSave)
                {
                    await cache.SetAsync(key.ToStringKey(), cacheItem);

                    staleState = await MarkAsNotStale(staleState);
                }


                return cacheItem;
            });

            return ConvertToListDto(cacheItem, input);
        }

        private static DateTime? GetMaxDateTime(IEnumerable<TItem> items)
        {
            return items.Max(x => x.LastInteractionTime);
        }

        private static List<TItem> FilterListByDate(List<TItem> list, DateTime? afterDateTime)
        {
            if (afterDateTime.HasValue)
            {
                return list
                    .Where(x => x.CreationTime > afterDateTime
                                || x.LastModificationTime > afterDateTime
                                || x.DeletionTime > afterDateTime)
                    .ToList();
            }
            else
            {
                return list
                    .Where(x => !x.IsDeleted)
                    .ToList();
            }
        }

        private ListCacheItemDto<TListKey, TItem, TItemKey> ConvertToListDto(TListCacheItem cacheItem, GetListCacheListInput<TListKey> input)
        {
            var hardInvalidate = input.ValidateCacheCreationDateTime(cacheItem.CacheCreationDateTime);

            return new ListCacheItemDto<TListKey, TItem, TItemKey>
            {
                Key = cacheItem.Key,
                Items = FilterListByDate(cacheItem.Items, input.AfterDateTime),
                CacheCreationDateTime = cacheItem.CacheCreationDateTime,
                MaxDateTime = GetMaxDateTime(cacheItem.Items),
                HardInvalidate = hardInvalidate,
            };
        }

        private async Task<List<TItem>> GetAllItemsFromDbWithUow(TListKey key, DateTime? afterDateTime = null)
        {
            return await WithUnitOfWorkAsync(async () =>
            {
                return await GetAllItemsFromDb(key, afterDateTime);
            });
        }

        protected abstract Task<List<TItem>> GetAllItemsFromDb(TListKey key, DateTime? afterDateTime = null);

        protected IQueryable<TEntity> ApplyDateFilter(IQueryable<TEntity> query, DateTime? afterDateTime)
        {
            return query
                .WhereIf(afterDateTime.HasValue,
                    d => d.CreationTime > afterDateTime
                         || d.LastModificationTime > afterDateTime
                         || d.DeletionTime > afterDateTime);
        }

        #endregion

        #region Invalidation and Stale State

        protected abstract Task<TListKey> GetKeyFromEntity(TEntity entity);

        protected virtual async Task InvalidateCache(TEntity entity)
        {
            var key = await GetKeyFromEntity(entity);
            if (key == null)
            {
                return;
            }
            await InvalidateCache(key);
        }

        public async Task InvalidateCache(TListKey key)
        {
            await MarkAsStale(key);
        }

        public async Task HardInvalidateCache(TListKey key)
        {
            var keyString = key.ToStringKey();

            var cache = await GetCache();
            await cache.WithLockAsync(keyString, async () =>
            {
                await MarkAsStale(key);

                await cache.RemoveAsync(keyString);
                return null;
            });
        }

        private async Task<ListCacheStaleState> GetStaleState(TListKey key)
        {
            return await GetStaleState(key.ToStringKey());
        }

        private async Task<ListCacheStaleState> GetStaleState(string key)
        {
            var staleStateCache = await GetStaleStateCache();

            return await staleStateCache.GetAsync(key, () => Task.FromResult(new ListCacheStaleState
            {
                Key = key,
                StaleSinceDateTime = Clock.Now,
            }));
        }

        public async Task MarkAsStale(TListKey key)
        {
            var keyString = key.ToStringKey();

            var cache = await GetCache();
            await cache.SendCacheInvalidationInstructionAsync(keyString, hardInvalidate: false);

            await ScheduleSyncRequest(key);

            await MarkAsStaleInternal(key.ToStringKey());
        }

        public async Task MarkAsStaleInternal(string key)
        {
            var staleStateCache = await GetStaleStateCache();
            await staleStateCache.SetWithLockAsync(
                key,
                new ListCacheStaleState
                {
                    Key = key,
                    StaleSinceDateTime = Clock.Now,
                },
                _ => true //always mark as stale regardless of pre-lock value
            );
        }

        private async Task<ListCacheStaleState> MarkAsNotStale(ListCacheStaleState oldValue)
        {
            if (oldValue == null)
            {
                throw new ArgumentNullException(nameof(oldValue), "oldValue is required to avoid race condition when unmarking/marking cache as stale");
            }

            var staleStateCache = await GetStaleStateCache();
            return await staleStateCache.SetWithLockAsync(
                oldValue.Key,
                new ListCacheStaleState
                {
                    Key = oldValue.Key,
                    StaleSinceDateTime = null,
                },
                //only update if the stale state is still the same as when we started
                currentValue => currentValue == null || oldValue.StaleSinceDateTime == currentValue.StaleSinceDateTime
            );
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<TEntity> eventData)
        {
            await InvalidateCache(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityCreatedEventData<TEntity> eventData)
        {
            await InvalidateCache(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityDeletedEventData<TEntity> eventData)
        {
            await InvalidateCache(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityChangingEventData<TEntity> eventData)
        {
            if (UnitOfWorkManager.Current == null)
            {
                throw new ApplicationException($"Unit of work is missing on {typeof(TEntity).Name} Changing event");
            }

            var key = await GetKeyFromEntity(eventData.Entity);
            UnitOfWorkManager.Current.Failed += async (sender, args) =>
            {
                await HardInvalidateCache(key);
            };
            await MarkAsStale(key);
        }

        #endregion

        #region Settings

        public async Task<bool> IsEnabled()
        {
            try
            {
                if (SettingManager == null)
                {
                    return false;
                }
                return await SettingManager.GetSettingValueAsync<bool>(AppSettings.ListCaches.IsEnabled(CacheName, ListCacheSide.Backend));
            }
            catch (Exception ex)
            {
                Logger?.Error($"Failed to check if cache {CacheName} is enabled: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> IsFrontendCacheEnabled()
        {
            try
            {
                if (SettingManager == null)
                {
                    return false;
                }
                return await SettingManager.GetSettingValueAsync<bool>(AppSettings.ListCaches.IsEnabled(CacheName, ListCacheSide.Frontend));
            }
            catch (Exception ex)
            {
                Logger?.Error($"Failed to check if frontend cache {CacheName} is enabled: {ex.Message}", ex);
                return false;
            }
        }

        private async Task<TimeSpan> GetSlidingExpirationTime()
        {
            var valueInMinutes = await SettingManager.GetSettingValueAsync<int>(AppSettings.ListCaches.SlidingExpirationTimeMinutes(CacheName, ListCacheSide.Backend));
            return TimeSpan.FromMinutes(valueInMinutes);
        }

        private async Task<int> GetSyncRequestDebounceDelayMs()
        {
            return await SettingManager.GetSettingValueAsync<int>(AppSettings.ListCaches.SyncRequestDebounceDelayMs);
        }

        private async Task<T> ConfigureCache<T>(T cache)
            where T : ICacheOptions
        {
            cache.DefaultSlidingExpireTime = await GetSlidingExpirationTime();
            return cache;
        }

        #endregion

        #region ListCacheSyncRequests

        public async Task ScheduleSyncRequest(TListKey key)
        {
            if (!await IsFrontendCacheEnabled())
            {
                return;
            }
            var delay = TimeSpan.FromMilliseconds(await GetSyncRequestDebounceDelayMs());
            var timerKey = $"{CacheName}:{key.ToStringKey()}";
            int? tenantId = null;
            if (key is ListCacheTenantKey tenantKey)
            {
                tenantId = tenantKey.TenantId;
            }
            _syncRequestDebounceTimers.AddOrUpdate(timerKey,
                // Create new timer if one doesn't exist
                _ => new Timer(_ =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await SignalRCommunicator.SendListCacheSyncRequest(new ListCacheSyncRequest<TListKey>
                            {
                                CacheName = CacheName,
                                Key = key,
                                TenantId = tenantId,
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error sending ListCacheSyncRequest: {ex.Message}", ex);
                        }
                    }).ConfigureAwait(false);
                }, null, delay, Timeout.InfiniteTimeSpan),
                // Reset the timer if it already exists
                (_, existingTimer) =>
                {
                    existingTimer.Change(delay, Timeout.InfiniteTimeSpan);
                    return existingTimer;
                });
        }

        #endregion

        #region Misc

        private ITypedCache<string, TListCacheItem> _cache;
        protected async Task<ITypedCache<string, TListCacheItem>> GetCache()
        {
            return _cache ??=
                await ConfigureCache(
                    CacheManager
                        .GetCache(CacheName)
                        .UseListCacheLogic()
                        .AsTyped<string, TListCacheItem>()
                );
        }

        private ITypedCache<string, ListCacheStaleState> _staleStateCache;
        protected async Task<ITypedCache<string, ListCacheStaleState>> GetStaleStateCache()
        {
            return _staleStateCache ??=
                await ConfigureCache(
                    CacheManager
                        .GetCache(CacheName + StaleStateCacheSuffix)
                        .SuppressInvalidationSyncCalls()
                        .AsTyped<string, ListCacheStaleState>()
                );
        }

        public async Task<TResult> WithUnitOfWorkAsync<TResult>(IMustHaveTenant entity, Func<Task<TResult>> action)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (UnitOfWorkManager.Current.SetTenantId(entity.TenantId)) //this is needed for "send order to hauling company" functionality
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    return await action();
                }
            }, new UnitOfWorkOptions { IsTransactional = false });
        }

        public async Task<TResult> WithUnitOfWorkAsync<TResult>(Func<Task<TResult>> action)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    return await action();
                }
            }, new UnitOfWorkOptions { IsTransactional = false });
        }

        #endregion

    }
}
