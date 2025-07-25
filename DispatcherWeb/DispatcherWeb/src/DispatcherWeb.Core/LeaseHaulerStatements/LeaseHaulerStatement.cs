using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.LeaseHaulerStatements
{
    [Table("LeaseHaulerStatement")]
    public class LeaseHaulerStatement : FullAuditedEntity, IMustHaveTenant
    {
        public LeaseHaulerStatement()
        {

            LeaseHaulerStatementTickets = new HashSet<LeaseHaulerStatementTicket>();
        }

        public int TenantId { get; set; }

        public DateTime StatementDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ICollection<LeaseHaulerStatementTicket> LeaseHaulerStatementTickets { get; set; }
    }
}
