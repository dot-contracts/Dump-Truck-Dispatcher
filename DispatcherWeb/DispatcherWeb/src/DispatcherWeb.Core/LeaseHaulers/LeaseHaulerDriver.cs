using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.LeaseHaulers
{
    [Table("LeaseHaulerDriver")]
    public class LeaseHaulerDriver : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public int LeaseHaulerId { get; set; }
        public int DriverId { get; set; }
        public virtual LeaseHauler LeaseHauler { get; set; }
        public virtual Driver Driver { get; set; }
    }
}
