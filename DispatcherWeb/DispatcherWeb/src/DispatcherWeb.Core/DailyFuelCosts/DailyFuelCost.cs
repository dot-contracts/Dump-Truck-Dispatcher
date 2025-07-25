using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.DailyFuelCosts
{
    [Table("DailyFuelCost")]
    public class DailyFuelCost : FullAuditedEntity, IMustHaveTenant
    {
        public DateTime Date { get; set; }

        [Column(TypeName = "money")]
        public decimal Cost { get; set; }

        public int TenantId { get; set; }
    }
}
