using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.PayStatements
{
    [Table("PayStatementDriverDateConflict")]
    public class PayStatementDriverDateConflict : Entity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public int PayStatementId { get; set; }
        public virtual PayStatement PayStatement { get; set; }
        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }
        public DateTime Date { get; set; }
        public DriverDateConflictKind ConflictKind { get; set; }
    }
}
