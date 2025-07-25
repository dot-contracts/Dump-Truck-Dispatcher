using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Locations
{
    [Table("LocationCategory")]
    public class LocationCategory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public PredefinedLocationCategoryKind? PredefinedLocationCategoryKind { get; set; }
    }
}
