using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerTruckCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerId { get; set; }
        public int? TruckId { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
    }
}
