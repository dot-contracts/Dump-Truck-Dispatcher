using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.HaulZones
{
    [Table("HaulZone")]
    public class HaulZone : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.HaulZone.Name)]
        public string Name { get; set; }

        public int UnitOfMeasureId { get; set; }

        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        public float Quantity { get; set; }

        public decimal? BillRatePerTon { get; set; }

        public decimal? MinPerLoad { get; set; }

        public decimal? PayRatePerTon { get; set; }

        public bool IsActive { get; set; }
    }
}
