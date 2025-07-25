using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("VehicleService")]
    public class VehicleService : FullAuditedEntity, IMustHaveTenant
    {
        public VehicleService()
        {
            Documents = new List<VehicleServiceDocument>();
        }

        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int? RecommendedTimeInterval { get; set; }
        public decimal? RecommendedHourInterval { get; set; }
        public int? RecommendedMileageInterval { get; set; }

        public int? WarningDays { get; set; }
        public decimal? WarningHours { get; set; }
        public int? WarningMiles { get; set; }

        public ICollection<VehicleServiceDocument> Documents { get; set; }
        public ICollection<PreventiveMaintenance> PreventiveMaintenance { get; set; }
    }
}
