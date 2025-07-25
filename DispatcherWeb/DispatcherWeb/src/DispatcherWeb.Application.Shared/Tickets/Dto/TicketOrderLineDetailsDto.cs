namespace DispatcherWeb.Tickets.Dto
{
    public class TicketOrderLineDetailsDto
    {
        public DesignationEnum Designation { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public int? FreightUomId { get; set; }
        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? MaterialItemId { get; set; }
        public int? FreightItemId { get; set; }
        public bool CalculateMinimumFreightAmount { get; set; }
        public decimal MinimumFreightAmount { get; set; }
    }
}
