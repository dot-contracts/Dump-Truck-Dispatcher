using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class RevenueGraphDataItemFromTickets : RevenueGraphDataItem, ITicketQuantity
    {
        public decimal? TicketFreightQuantity { get; set; }
        public decimal? TicketMaterialQuantity { get; set; }
        decimal? ITicketQuantity.FreightQuantity => TicketFreightQuantity;
        decimal? ITicketQuantity.MaterialQuantity => TicketMaterialQuantity;

        public DesignationEnum Designation { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public override decimal FreightQuantity { get => this.GetFreightQuantity() ?? 0; }
        public override decimal MaterialQuantity { get => this.GetMaterialQuantity() ?? 0; }
    }
}
