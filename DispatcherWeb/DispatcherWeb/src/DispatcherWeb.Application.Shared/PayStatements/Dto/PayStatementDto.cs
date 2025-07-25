using System;
using System.Collections.Generic;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementDto
    {
        public int Id { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeProductionPay { get; set; }
        public bool IncludeHourly { get; set; }
        public bool IncludeSalary { get; set; }
        public List<PayStatementDriverDateConflictDto> DriverDateConflicts { get; set; }
    }
}
