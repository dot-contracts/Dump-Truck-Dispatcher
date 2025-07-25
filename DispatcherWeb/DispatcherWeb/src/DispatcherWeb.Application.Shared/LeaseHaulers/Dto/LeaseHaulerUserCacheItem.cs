using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerUserCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerId { get; set; }
        public long UserId { get; set; }
    }
}
