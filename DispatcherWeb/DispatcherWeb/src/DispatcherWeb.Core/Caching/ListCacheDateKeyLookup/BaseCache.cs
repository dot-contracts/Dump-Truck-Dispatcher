using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService
    {
        public abstract class BaseCache<TKey, TParentKey, TEntity> :
            IAsyncEventHandler<EntityCreatingEventData<TEntity>>,
            IAsyncEventHandler<EntityUpdatingEventData<TEntity>>,
            IAsyncEventHandler<EntityDeletingEventData<TEntity>>,
            IAsyncEventHandler<EntityChangedEventData<TEntity>>
            where TEntity : Entity<TKey>
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            private readonly IRepository<TEntity, TKey> _repository;

            protected const string CacheNamePrefix = "ListCacheDateKeyLookup.";
            protected abstract string CacheNameSuffix { get; }
            public string CacheName => CacheNamePrefix + CacheNameSuffix;

            protected ISettingManager SettingManager { get; }
            protected ICacheManager CacheManager { get; }
            protected IUnitOfWorkManager UnitOfWorkManager { get; }


            public BaseCache(
                BaseCacheDependency baseCacheDependency,
                IRepository<TEntity, TKey> repository
            )
            {
                _repository = repository;
                SettingManager = baseCacheDependency.SettingManager;
                CacheManager = baseCacheDependency.CacheManager;
                UnitOfWorkManager = baseCacheDependency.UnitOfWorkManager;
            }

            public async Task<TParentKey> LookupParentKey(TKey childId)
            {
                var cache = await GetCache();
                var item = await cache.GetAsync(childId, async () =>
                {
                    return await WithUnitOfWorkAsync(async () =>
                    {
                        var key = await GetKeyFromDatabase((await _repository.GetQueryAsync())
                            .Where(x => x.Id.Equals(childId)));

                        return new KeyHolder<TParentKey>
                        {
                            Key = key,
                        };
                    });
                });

                return item.Key;
            }

            protected abstract Task<TParentKey> GetKeyFromDatabase(IQueryable<TEntity> queryable);

            private async Task<TParentKey> TryGetKeyForEntity(TEntity entity)
            {
                if (IsDefault(entity.Id))
                {
                    return default;
                }

                var cache = await GetCache();
                var itemOrNull = await cache.TryGetValueAsync(entity.Id);
                return itemOrNull.HasValue
                    ? itemOrNull.Value.Key
                    : default;
            }

            protected abstract bool IsKeyValidForEntity(TParentKey key, TEntity entity);

            //on date changes - run this manually
            public async Task InvalidateKeyForEntity(TEntity order)
            {
                var cache = await GetCache();
                await cache.RemoveAsync(order.Id);
            }

            //call on 'changing' to invalidate incorrect data
            public async Task InvalidateKeyForEntityIfExistsAndDifferent(TEntity entity)
            {
                if (IsDefault(entity.Id))
                {
                    return;
                }

                var cache = await GetCache();
                var key = await TryGetKeyForEntity(entity);

                if (key == null || IsKeyValidForEntity(key, entity))
                {
                    return;
                }

                await cache.RemoveAsync(entity.Id);
            }

            public async Task SetItemForEntityIfDoesntExistOrDifferent(TEntity entity)
            {
                if (IsDefault(entity.Id))
                {
                    return;
                }

                var cache = await GetCache();
                var item = await TryGetKeyForEntity(entity);
                if (item == null)
                {
                    await cache.SetAsync(entity.Id, CreateCacheItemForEntity(entity));
                    return;
                }

                if (!IsKeyValidForEntity(item, entity))
                {
                    await cache.RemoveAsync(entity.Id);
                    await cache.SetAsync(entity.Id, CreateCacheItemForEntity(entity));
                }
            }

            private bool IsDefault<T>(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            private KeyHolder<TParentKey> CreateCacheItemForEntity(TEntity entity)
            {
                return new KeyHolder<TParentKey>
                {
                    Key = CreateParentKeyForEntity(entity),
                };
            }

            protected abstract TParentKey CreateParentKeyForEntity(TEntity entity);

            private ITypedCache<TKey, KeyHolder<TParentKey>> _cache;

            protected async Task<ITypedCache<TKey, KeyHolder<TParentKey>>> GetCache()
            {
                return _cache ??=
                    await ConfigureCache(
                        CacheManager
                            .GetCache(CacheName)
                            .AsTyped<TKey, KeyHolder<TParentKey>>()
                    );
            }

            private async Task<T> ConfigureCache<T>(T cache)
                where T : ICacheOptions
            {
                cache.DefaultSlidingExpireTime = await GetSlidingExpirationTime();
                return cache;
            }

            private async Task<TimeSpan> GetSlidingExpirationTime()
            {
                var valueInMinutes = await SettingManager.GetSettingValueAsync<int>(AppSettings.ListCaches.DateKeyLookup.SlidingExpirationTimeMinutes);
                return TimeSpan.FromMinutes(valueInMinutes);
            }


            public async Task HandleEventAsync(EntityCreatingEventData<TEntity> eventData)
            {
                // "Creating" event can populate the record in either case - rejected UOW won't cause issues later
                await SetItemForEntityIfDoesntExistOrDifferent(eventData.Entity);
            }

            public async Task HandleEventAsync(EntityUpdatingEventData<TEntity> eventData)
            {
                await InvalidateKeyForEntityIfExistsAndDifferent(eventData.Entity);
            }

            public async Task HandleEventAsync(EntityDeletingEventData<TEntity> eventData)
            {
                await InvalidateKeyForEntityIfExistsAndDifferent(eventData.Entity);
            }

            public async Task HandleEventAsync(EntityChangedEventData<TEntity> eventData)
            {
                await SetItemForEntityIfDoesntExistOrDifferent(eventData.Entity);
            }

            public async Task<TResult> WithUnitOfWorkAsync<TResult>(IMustHaveTenant entity, Func<Task<TResult>> action)
            {
                return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (UnitOfWorkManager.Current.SetTenantId(entity.TenantId))
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

            protected class KeyHolder<T>
            {
                public T Key { get; set; }
            }
        }
    }
}
