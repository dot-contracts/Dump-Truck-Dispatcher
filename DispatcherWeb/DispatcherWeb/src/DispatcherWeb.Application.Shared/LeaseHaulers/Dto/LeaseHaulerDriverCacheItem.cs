using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerDriverCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerId { get; set; }
        public int DriverId { get; set; }
    }
}
