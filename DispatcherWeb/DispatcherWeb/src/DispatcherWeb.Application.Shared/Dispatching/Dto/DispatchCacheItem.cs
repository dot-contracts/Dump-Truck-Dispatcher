using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchCacheItem : AuditableCacheItem
    {
        public int OrderLineId { get; set; }
        public int? OrderLineTruckId { get; set; }
        public int TruckId { get; set; }
        public DispatchStatus Status { get; set; }
        public DateTime? Acknowledged { get; set; }
        public bool IsMultipleLoads { get; set; }
    }
}
