using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Orders;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.TimeClassifications
{
    [Table("TimeClassification")]
    public class TimeClassification : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public bool IsProductionBased { get; set; }

        public decimal? DefaultRate { get; set; }

        public virtual ICollection<EmployeeTimeClassification> EmployeeTimeClassifications { get; set; }

        public virtual ICollection<QuoteLine> QuoteLines { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual ICollection<PayStatementTicket> PayStatementTickets { get; set; }

        public virtual ICollection<PayStatementTime> PayStatementTimeRecords { get; set; }
    }
}
