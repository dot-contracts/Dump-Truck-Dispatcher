using Abp.Dependency;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Notifications.Cache
{
    public class PriorityUserNotificationCache : UserNotificationCacheBase, IPriorityUserNotificationCache, ISingletonDependency
    {

        public const string CacheNameConst = "PriorityUserNotification-Cache";
        public override string CacheName => CacheNameConst;

        public PriorityUserNotificationCache(ICacheManager cacheManager)
            : base(cacheManager)
        {
        }
    }
}
