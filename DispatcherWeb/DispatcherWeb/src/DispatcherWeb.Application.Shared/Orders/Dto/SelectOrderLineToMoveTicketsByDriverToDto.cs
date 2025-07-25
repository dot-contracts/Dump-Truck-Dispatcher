namespace DispatcherWeb.Orders.Dto
{
    public class SelectOrderLineToMoveTicketsByDriverToDto : GetOrderLinesSelectListToMoveTicketsByDriverToInput
    {
        public string CustomerName { get; set; }
    }
}
