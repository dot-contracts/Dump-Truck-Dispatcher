using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserCacheSynchronizer :
        IAsyncEventHandler<EntityDeletedEventData<User>>,
        IAsyncEventHandler<EntityUpdatedEventData<User>>,
        ITransientDependency
    {
        private readonly ICacheManager _cacheManager;

        public UserCacheSynchronizer(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        private ITypedCache<long, UserCacheItem> GetUserCacheInternal()
        {
            return _cacheManager
                .GetCache(UserCacheItem.CacheName)
                .AsTyped<long, UserCacheItem>();
        }

        public async Task HandleEventAsync(EntityDeletedEventData<User> eventData)
        {
            await RemoveCacheItem(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<User> eventData)
        {
            await RemoveCacheItem(eventData.Entity);
        }

        private async Task RemoveCacheItem(User user)
        {
            await GetUserCacheInternal().RemoveAsync(user.Id);
        }
    }
}
