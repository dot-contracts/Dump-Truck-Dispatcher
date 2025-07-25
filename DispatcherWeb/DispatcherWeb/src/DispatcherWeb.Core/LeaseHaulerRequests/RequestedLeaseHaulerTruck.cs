using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [Table("RequestedLeaseHaulerTruck")]
    public class RequestedLeaseHaulerTruck : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int LeaseHaulerRequestId { get; set; }
        public LeaseHaulerRequest LeaseHaulerRequest { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }
    }
}
