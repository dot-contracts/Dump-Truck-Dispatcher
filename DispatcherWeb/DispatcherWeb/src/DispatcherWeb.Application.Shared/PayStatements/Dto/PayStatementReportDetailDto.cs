using System.Collections.Generic;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportDetailDto
    {
        public string EmployeeId { get; set; }

        public string DriverName { get; set; }

        public decimal Total { get; set; }

        public List<PayStatementReportTicketDto> Tickets { get; set; }

        public List<PayStatementReportTimeDto> TimeRecords { get; set; }

        public decimal TimeBasedTotal { get; set; }

        public decimal ProductionBasedTotal { get; set; }
    }
}