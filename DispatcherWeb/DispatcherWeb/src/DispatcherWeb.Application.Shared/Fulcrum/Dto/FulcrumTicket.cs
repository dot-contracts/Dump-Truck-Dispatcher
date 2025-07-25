using System;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumTicket
    {
        public Guid Id { get; set; }

        public decimal TareWeight { get; set; }

        public decimal NetWeight { get; set; }

        public DateTime TicketTime { get; set; }

        public int TicketNumber { get; set; }

        public int DispatchId { get; set; }

    }
}
