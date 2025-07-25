using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Trucks
{
    [Table("VehicleUsage")]
    public class VehicleUsage : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public DateTime ReadingDateTime { get; set; }
        public ReadingType ReadingType { get; set; }
        public decimal Reading { get; set; }

    }
}
