using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class RequestedLeaseHaulerTruckCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerRequestId { get; set; }
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
    }
}
