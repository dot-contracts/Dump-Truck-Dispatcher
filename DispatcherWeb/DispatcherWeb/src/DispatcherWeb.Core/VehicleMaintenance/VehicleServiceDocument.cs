using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("VehicleServiceDocument")]
    public class VehicleServiceDocument : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int VehicleServiceId { get; set; }
        public VehicleService VehicleService { get; set; }

        public Guid FileId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }
    }
}
