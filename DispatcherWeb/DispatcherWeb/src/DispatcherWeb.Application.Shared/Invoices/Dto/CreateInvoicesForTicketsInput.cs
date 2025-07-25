using System.Collections.Generic;

namespace DispatcherWeb.Invoices.Dto
{
    public class CreateInvoicesForTicketsInput
    {
        public int? OfficeId { get; set; }
        public List<int> TicketIds { get; set; }
    }
}
