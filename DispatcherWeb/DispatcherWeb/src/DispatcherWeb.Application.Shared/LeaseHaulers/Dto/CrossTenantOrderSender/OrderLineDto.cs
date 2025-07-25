using System;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class OrderLineDto
    {
        public int Id { get; set; }
        public int? HaulingCompanyOrderLineId { get; set; }
        public int? HaulingCompanyTenantId { get; set; }
        public int? MaterialCompanyOrderLineId { get; set; }
        public int? MaterialCompanyTenantId { get; set; }
        public LocationDto DeliverTo { get; set; }
        public LocationDto LoadAt { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public short? TravelTime { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public UnitOfMeasureDto FreightUom { get; set; }
        public UnitOfMeasureDto MaterialUom { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsComplete { get; set; }
        public bool IsMultipleLoads { get; set; }
        public int LineNumber { get; set; }
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public double? NumberOfTrucks { get; set; }
        public OrderDto Order { get; set; }
        public DesignationEnum Designation { get; set; }
        public ItemDto FreightItem { get; set; }
        public ItemDto MaterialItem { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public StaggeredTimeKind StaggeredTimeKind { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public bool RequireTicket { get; set; }
    }
}
