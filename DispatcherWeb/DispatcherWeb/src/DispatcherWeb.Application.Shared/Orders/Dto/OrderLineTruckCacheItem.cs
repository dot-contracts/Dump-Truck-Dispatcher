using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineTruckCacheItem : AuditableCacheItem
    {
        public int? ParentOrderLineTruckId { get; set; }
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
        public int OrderLineId { get; set; }
        public decimal Utilization { get; set; }
        public bool IsDone { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public int? TrailerId { get; set; }
    }
}
