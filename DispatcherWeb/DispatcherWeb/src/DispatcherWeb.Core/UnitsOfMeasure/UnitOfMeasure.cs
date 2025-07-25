using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.UnitsOfMeasure
{
    [Table("UnitOfMeasure")]
    public class UnitOfMeasure : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.UnitOfMeasure.Name)]
        public string Name { get; set; }

        public int? UnitOfMeasureBaseId { get; set; }

        public virtual UnitOfMeasureBase UnitOfMeasureBase { get; set; }
    }
}
