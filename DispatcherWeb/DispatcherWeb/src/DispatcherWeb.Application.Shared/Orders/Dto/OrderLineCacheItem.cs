using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineCacheItem : AuditableCacheItem
    {
        public int OrderId { get; set; }
        public bool IsTimeStaggered { get; set; }
        public bool IsTimeEditable { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public StaggeredTimeKind StaggeredTimeKind { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public int? MaterialItemId { get; set; }
        public int? FreightItemId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public bool IsFreightPriceOverridden { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public DesignationEnum Designation { get; set; }
        public double? NumberOfTrucks { get; set; }
        public double? ScheduledTrucks { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
        public int? HaulingCompanyOrderLineId { get; set; }
        public int? MaterialCompanyOrderLineId { get; set; }
    }
}
