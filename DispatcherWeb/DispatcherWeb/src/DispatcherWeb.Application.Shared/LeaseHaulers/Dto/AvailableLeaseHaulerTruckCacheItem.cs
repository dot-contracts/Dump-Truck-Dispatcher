using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class AvailableLeaseHaulerTruckCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerId { get; set; }
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
        public int? OfficeId { get; set; }
    }
}
