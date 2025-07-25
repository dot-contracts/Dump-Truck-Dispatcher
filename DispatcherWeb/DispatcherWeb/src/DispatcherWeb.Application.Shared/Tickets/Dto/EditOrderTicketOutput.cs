using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Tickets.Dto
{
    public class EditOrderTicketOutput
    {
        public OrderTicketEditDto Ticket { get; set; }
        public IOrderTaxDetails OrderTaxDetails { get; set; }
        public string WarningText { get; set; }
    }
}
