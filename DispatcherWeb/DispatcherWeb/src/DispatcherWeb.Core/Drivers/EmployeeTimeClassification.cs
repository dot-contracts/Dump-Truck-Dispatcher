using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.TimeClassifications;

namespace DispatcherWeb.Drivers
{
    [Table("EmployeeTimeClassification")]
    public class EmployeeTimeClassification : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int DriverId { get; set; }

        public virtual Driver Driver { get; set; }

        public int TimeClassificationId { get; set; }

        public TimeClassification TimeClassification { get; set; }

        public bool IsDefault { get; set; }

        public bool AllowForManualTime { get; set; }

        public decimal PayRate { get; set; }
    }
}
