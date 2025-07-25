using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class LeaseHaulerStatementDto
    {
        public int Id { get; set; }

        public DateTime StatementDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<LeaseHaulerStatementCustomerDto> Customers { get; set; }
    }
}
