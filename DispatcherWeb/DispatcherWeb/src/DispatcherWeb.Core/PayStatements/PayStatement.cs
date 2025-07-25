using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.PayStatements
{
    [Table("PayStatement")]
    public class PayStatement : FullAuditedEntity, IMustHaveTenant
    {
        public PayStatement()
        {
            PayStatementDetails = new HashSet<PayStatementDetail>();
        }
        public int TenantId { get; set; }
        public int? OfficeId { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeProductionPay { get; set; }
        public bool IncludeHourly { get; set; }
        public bool IncludeSalary { get; set; }
        public virtual ICollection<PayStatementDetail> PayStatementDetails { get; set; }

        public virtual ICollection<PayStatementDriverDateConflict> PayStatementDriverDateConflicts { get; set; }
    }
}
