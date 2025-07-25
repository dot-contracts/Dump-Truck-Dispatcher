using System;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteDeliveryRawDto : TicketQuantityDto
    {
        public DateTime? Date { get; set; }
    }
}
