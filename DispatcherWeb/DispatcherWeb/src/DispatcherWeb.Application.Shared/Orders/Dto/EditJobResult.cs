namespace DispatcherWeb.Orders.Dto
{
    public class EditJobResult : EditOrderResult
    {
        public int? OrderLineId { get; set; }
        public int? TicketId { get; set; }
    }
}
