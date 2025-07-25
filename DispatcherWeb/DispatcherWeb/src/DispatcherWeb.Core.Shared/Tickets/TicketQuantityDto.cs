namespace DispatcherWeb.Tickets
{
    public class TicketQuantityDto : ITicketQuantity
    {
        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? OrderLineMaterialUomId { get; set; }

        public int? OrderLineFreightUomId { get; set; }

        public int? TicketUomId { get; set; }

        public decimal FuelSurcharge { get; set; }

        public int TicketId { get; set; }
    }
}
