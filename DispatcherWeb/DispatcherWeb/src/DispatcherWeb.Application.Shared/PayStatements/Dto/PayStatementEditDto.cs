using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementEditDto
    {
        public int Id { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
