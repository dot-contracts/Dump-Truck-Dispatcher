using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class AddPayStatementInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeProductionPay { get; set; }
        public bool IncludeHourly { get; set; }
        public bool IncludeSalary { get; set; }
        public bool IncludePastTickets { get; set; }
        public int? OfficeId { get; set; }
        public bool LocalEmployeesOnly { get; set; }
    }
}
