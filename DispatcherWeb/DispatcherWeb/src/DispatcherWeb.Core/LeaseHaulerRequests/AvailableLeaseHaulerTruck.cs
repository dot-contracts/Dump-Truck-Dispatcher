using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [Table("AvailableLeaseHaulerTruck")]
    public class AvailableLeaseHaulerTruck : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? LeaseHaulerRequestId { get; set; }
        public LeaseHaulerRequest LeaseHaulerRequest { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }

        public Office Office { get; set; }

        public int LeaseHaulerId { get; set; }

        public LeaseHauler LeaseHauler { get; set; }

        public int TruckId { get; set; }

        public virtual Truck Truck { get; set; }

        public int DriverId { get; set; }

        public virtual Driver Driver { get; set; }
    }
}
