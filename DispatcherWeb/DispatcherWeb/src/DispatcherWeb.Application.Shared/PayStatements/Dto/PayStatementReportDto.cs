using System;
using System.Collections.Generic;
using System.Globalization;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportDto
    {
        public int Id { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeProductionPay { get; set; }
        public bool IncludeHourly { get; set; }
        public bool IncludeSalary { get; set; }
        public List<PayStatementReportDetailDto> Drivers { get; set; }

        public CultureInfo CurrencyCulture { get; set; }
    }
}
