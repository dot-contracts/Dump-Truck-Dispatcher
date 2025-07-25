using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchCompleteInfoLoadDto
    {
        public int Id { get; set; }

        public Guid? SignatureId { get; set; }

        public DateTime? SourceDateTime { get; set; }

        public DispatchCompleteInfoTicketDto LastTicket { get; set; }

    }
}
