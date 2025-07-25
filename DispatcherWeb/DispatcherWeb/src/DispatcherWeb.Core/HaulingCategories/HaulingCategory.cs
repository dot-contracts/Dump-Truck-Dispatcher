using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Items;
using DispatcherWeb.UnitsOfMeasure;
using DispatcherWeb.VehicleCategories;

namespace DispatcherWeb.HaulingCategories
{
    [Table("HaulingCategory")]
    public class HaulingCategory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int ItemId { get; set; }

        public virtual Item Item { get; set; }

        public int? TruckCategoryId { get; set; }

        public virtual VehicleCategory TruckCategory { get; set; }

        public int UnitOfMeasureId { get; set; }

        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        public decimal MinimumBillableUnits { get; set; }

        public decimal LeaseHaulerRate { get; set; }

        public virtual ICollection<HaulingCategoryPrice> HaulingCategoryPrices { get; set; }
    }
}
