namespace DispatcherWeb.Tickets.Dto
{
    public class TicketEditQuantityDto : ITicketEditQuantity
    {
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public int? FreightUomId { get; set; }
        public int? FreightItemId { get; set; }
        public int? MaterialItemId { get; set; }
    }
}
