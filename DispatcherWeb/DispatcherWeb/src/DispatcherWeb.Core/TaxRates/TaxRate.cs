using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.TaxRates
{
    [Table("TaxRate")]
    public class TaxRate : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.TaxRate.Name)]
        public string Name { get; set; }

        public decimal Rate { get; set; }
    }
}
