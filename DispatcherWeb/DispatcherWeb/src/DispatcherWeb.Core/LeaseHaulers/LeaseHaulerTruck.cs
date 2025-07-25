using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.LeaseHaulers
{
    [Table("LeaseHaulerTruck")]
    public class LeaseHaulerTruck : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public int LeaseHaulerId { get; set; }
        public int TruckId { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
        public virtual LeaseHauler LeaseHauler { get; set; }
        public virtual Truck Truck { get; set; }
    }
}
