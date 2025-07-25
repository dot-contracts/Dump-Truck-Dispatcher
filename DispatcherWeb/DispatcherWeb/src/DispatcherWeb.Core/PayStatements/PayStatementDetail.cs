using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.PayStatements
{
    [Table("PayStatementDetail")]
    public class PayStatementDetail : CreationAuditedEntity, IMustHaveTenant
    {
        public PayStatementDetail()
        {
            PayStatementTickets = new HashSet<PayStatementTicket>();
            PayStatementTimeRecords = new HashSet<PayStatementTime>();
        }
        public int TenantId { get; set; }
        public int PayStatementId { get; set; }
        public virtual PayStatement PayStatement { get; set; }
        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        public decimal ProductionBasedTotal { get; set; }
        public decimal TimeBasedTotal { get; set; }

        [Column(TypeName = "money")]
        public decimal Total { get; set; }

        public virtual ICollection<PayStatementTicket> PayStatementTickets { get; set; }
        public virtual ICollection<PayStatementTime> PayStatementTimeRecords { get; set; }
    }
}
