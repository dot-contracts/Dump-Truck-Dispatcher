using System;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto
{
    public class RevenueBreakdownByTruckItemRaw : ITicketQuantity
    {
        public DateTime? DeliveryDate { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public Shift? Shift { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public decimal FreightPriceOriginal { get; set; }
        public decimal MaterialPriceOriginal { get; set; }
        public bool IsFreightPriceOverridden { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public decimal? OrderLineTicketsFreightQuantitySum { get; set; }
        public decimal? OrderLineTicketsMaterialQuantitySum { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal PercentFreightQtyForTicket => OrderLineTicketsFreightQuantitySum > 0 && FreightQuantity.HasValue ? FreightQuantity.Value / OrderLineTicketsFreightQuantitySum.Value : 0;
        public decimal PercentMaterialQtyForTicket => OrderLineTicketsMaterialQuantitySum > 0 && MaterialQuantity.HasValue ? MaterialQuantity.Value / OrderLineTicketsMaterialQuantitySum.Value : 0;

        public decimal ActualMaterialQuantity => this.GetMaterialQuantity() ?? 0;
        public decimal ActualFreightQuantity => this.GetFreightQuantity() ?? 0;
    }
}
