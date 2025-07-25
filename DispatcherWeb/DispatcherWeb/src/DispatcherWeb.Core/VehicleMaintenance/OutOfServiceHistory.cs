using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("OutOfServiceHistory")]
    public class OutOfServiceHistory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }

        public Truck Truck { get; set; }

        public DateTime OutOfServiceDate { get; set; }

        public DateTime? InServiceDate { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }
    }
}
