using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Locations
{
    [Table("LocationContact")]
    public class LocationContact : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        [Required]
        public int LocationId { get; set; }

        public int? MergedToId { get; set; }

        public virtual Location Location { get; set; }
    }
}
