using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class LeaseHaulerStatementReportDto
    {
        public int? Id { get; set; }
        public string FileName { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<LeaseHaulerStatementTicketReportDto> Tickets { get; set; }

        public LeaseHaulerStatementReportDto Clone()
        {
            return new LeaseHaulerStatementReportDto
            {
                Id = Id,
                FileName = FileName,
                StatementDate = StatementDate,
                StartDate = StartDate,
                EndDate = EndDate,
                Tickets = Tickets.ToList(),
            };
        }
    }
}
