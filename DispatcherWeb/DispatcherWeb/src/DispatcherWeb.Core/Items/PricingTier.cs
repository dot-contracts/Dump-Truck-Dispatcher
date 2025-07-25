using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Customers;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Items
{
    [Table("PricingTier")]
    public class PricingTier : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.PricingTier.Name)]
        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public virtual ICollection<ProductLocationPrice> ProductLocationPrices { get; set; }

        //todo add HaulingCategoryPrices

        public virtual ICollection<Customer> Customers { get; set; }
    }
}
