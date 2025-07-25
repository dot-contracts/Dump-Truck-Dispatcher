using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    [Table("FuelSurchargeCalculation")]
    public class FuelSurchargeCalculation : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.FuelSurchargeCalculation.Name)]
        public string Name { get; set; }

        public FuelSurchargeCalculationType Type { get; set; }

        [Column(TypeName = "money")]
        public decimal BaseFuelCost { get; set; }

        public bool CanChangeBaseFuelCost { get; set; }

        [Column(TypeName = "money")]
        public decimal Increment { get; set; }

        public decimal FreightRatePercent { get; set; }

        public bool Credit { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
