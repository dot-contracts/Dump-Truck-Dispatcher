using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementDriverDateConflictDto
    {
        public DateTime Date { get; set; }
        public string DriverName { get; set; }
        public DriverDateConflictKind ConflictKind { get; set; }
    }
}
