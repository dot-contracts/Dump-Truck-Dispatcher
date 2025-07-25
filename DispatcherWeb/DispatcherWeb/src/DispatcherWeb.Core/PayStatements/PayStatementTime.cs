using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;

namespace DispatcherWeb.PayStatements
{
    [Table("PayStatementTime")]
    public class PayStatementTime : CreationAuditedEntity, IMustHaveTenant
    {
        public PayStatementTime()
        {
            EmployeeTimeRecords = new HashSet<EmployeeTimePayStatementTime>();
        }

        public int TenantId { get; set; }
        public int PayStatementDetailId { get; set; }
        public virtual PayStatementDetail PayStatementDetail { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public virtual ICollection<EmployeeTimePayStatementTime> EmployeeTimeRecords { get; set; }
        public int TimeClassificationId { get; set; }
        public virtual TimeClassification TimeClassification { get; set; }
        public int? OrderLineId { get; set; }
        public virtual OrderLine OrderLine { get; set; }
        public decimal? DriverPayRate { get; set; }
    }
}
