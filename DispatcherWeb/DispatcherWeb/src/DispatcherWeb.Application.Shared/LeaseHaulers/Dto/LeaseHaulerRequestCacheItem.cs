using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerRequestCacheItem : AuditableCacheItem
    {
        public int? OrderLineId { get; set; }
        public int LeaseHaulerId { get; set; }
        public int? NumberTrucksRequested { get; set; }
        public LeaseHaulerRequestStatus? Status { get; set; }
    }
}
