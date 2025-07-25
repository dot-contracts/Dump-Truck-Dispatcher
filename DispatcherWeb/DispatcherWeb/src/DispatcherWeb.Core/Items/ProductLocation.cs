using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Locations;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Items
{
    [Table("ProductLocation")]
    public class ProductLocation : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int ItemId { get; set; }

        public int? LocationId { get; set; }

        public int? UnitOfMeasureId { get; set; }

        public decimal? Cost { get; set; }

        public virtual Location Location { get; set; }

        public virtual Item Item { get; set; }

        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        public virtual ICollection<ProductLocationPrice> ProductLocationPrices { get; set; }
    }
}
