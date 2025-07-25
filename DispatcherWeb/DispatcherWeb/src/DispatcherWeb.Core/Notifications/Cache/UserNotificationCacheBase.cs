using System.Threading.Tasks;
using Abp;
using Abp.Runtime.Caching;
using DispatcherWeb.Notifications.Dto;

namespace DispatcherWeb.Notifications.Cache
{
    public abstract class UserNotificationCacheBase : IUserNotificationCacheBase
    {
        public abstract string CacheName { get; }
        protected readonly ICacheManager _cacheManager;

        public UserNotificationCacheBase(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task<GetNotificationsOutput> GetFromCacheOrDefault(UserIdentifier userIdentifier)
        {
            var cache = GetUserNotificationCache();
            var result = await cache.TryGetValueAsync(userIdentifier.ToString());
            if (result.HasValue)
            {
                return result.Value;
            }
            return null;
        }

        public async Task StoreInCache(UserIdentifier userIdentifier, GetNotificationsOutput value)
        {
            var cache = GetUserNotificationCache();
            await cache.SetAsync(userIdentifier.ToString(), value);
        }

        public void RemoveFromCache(UserIdentifier userIdentifier)
        {
            var cache = GetUserNotificationCache();
            cache.Remove(userIdentifier.ToString());
        }

        public async Task RemoveFromCacheAsync(UserIdentifier userIdentifier)
        {
            var cache = GetUserNotificationCache();
            await cache.RemoveAsync(userIdentifier.ToString());
        }

        private ITypedCache<string, GetNotificationsOutput> GetUserNotificationCache()
        {
            return _cacheManager
                .GetCache(CacheName)
                .AsTyped<string, GetNotificationsOutput>();
        }
    }
}
