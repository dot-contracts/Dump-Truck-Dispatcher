using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Drivers
{
    [Table("DriverAssignment")]
    public class DriverAssignment : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public DateTime? StartTime { get; set; }

        public int? OfficeId { get; set; }

        public virtual Office Office { get; set; }

        public int TruckId { get; set; }

        public virtual Truck Truck { get; set; }

        public int? DriverId { get; set; }

        public virtual Driver Driver { get; set; }
    }
}
