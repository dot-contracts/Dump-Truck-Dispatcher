using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Items;

namespace DispatcherWeb.HaulingCategories
{
    [Table("HaulingCategoryPrice")]
    public class HaulingCategoryPrice : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int HaulingCategoryId { get; set; }

        public virtual HaulingCategory HaulingCategory { get; set; }

        public int PricingTierId { get; set; }

        public virtual PricingTier PricingTier { get; set; }

        public decimal? PricePerUnit { get; set; }
    }
}
