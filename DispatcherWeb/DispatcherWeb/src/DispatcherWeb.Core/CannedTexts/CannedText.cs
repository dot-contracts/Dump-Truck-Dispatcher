using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices;

namespace DispatcherWeb.CannedTexts
{
    [Table("CannedText")]
    public class CannedText : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(100)]
        public string Name { get; set; }

        public int OfficeId { get; set; }

        [StringLength(EntityStringFieldLengths.CannedText.Text)]
        public string Text { get; set; }

        public virtual Office Office { get; set; }
    }
}
