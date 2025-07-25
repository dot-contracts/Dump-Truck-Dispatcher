using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.VehicleCategories;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteLineVehicleCategory")]
    public class QuoteLineVehicleCategory : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int QuoteLineId { get; set; }
        public virtual QuoteLine QuoteLine { get; set; }

        public int VehicleCategoryId { get; set; }
        public virtual VehicleCategory VehicleCategory { get; set; }
    }
}
