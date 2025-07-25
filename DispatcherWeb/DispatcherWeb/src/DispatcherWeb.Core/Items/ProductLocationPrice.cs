using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Items
{
    [Table("ProductLocationPrice")]
    public class ProductLocationPrice : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int ProductLocationId { get; set; }

        public int PricingTierId { get; set; }

        public decimal? PricePerUnit { get; set; }

        public virtual ProductLocation ProductLocation { get; set; }

        public virtual PricingTier PricingTier { get; set; }
    }
}
