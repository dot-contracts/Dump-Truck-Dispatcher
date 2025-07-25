using System.Collections.Generic;

namespace DispatcherWeb.Imports.Dto
{
    public class ValidateTicketEarningsFileResult
    {
        public bool IsValid { get; set; }
        public List<DuplicateTicket> DuplicateTickets { get; set; } = new List<DuplicateTicket>();
        public int TotalRecordCount { get; internal set; }

        public class DuplicateTicket
        {
            public int Id { get; set; }
            public string Site { get; set; }
            public string TicketNumber { get; set; }
        }
    }
}
