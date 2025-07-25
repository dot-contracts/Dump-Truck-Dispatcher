using Abp.Dependency;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Notifications.Cache
{
    public class Top3UserNotificationCache : UserNotificationCacheBase, ITop3UserNotificationCache, ISingletonDependency
    {
        public const string CacheNameConst = "Top3UserNotification-Cache";
        public override string CacheName => CacheNameConst;

        public Top3UserNotificationCache(ICacheManager cacheManager)
            : base(cacheManager)
        {
        }
    }
}
