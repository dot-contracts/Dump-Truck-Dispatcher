namespace DispatcherWeb.Tickets.Dto
{
    public class MoveTicketsToOrderLineInput
    {
        public int? FromDriverId { get; set; }
        public int FromOrderLineId { get; set; }
        public int ToOrderLineId { get; set; }
    }
}
