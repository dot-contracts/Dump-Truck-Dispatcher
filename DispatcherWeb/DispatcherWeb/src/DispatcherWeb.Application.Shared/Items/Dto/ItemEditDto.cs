using System.Collections.Generic;
using DispatcherWeb.PricingTiers.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class ItemEditDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ItemType? Type { get; set; }

        public bool IsTaxable { get; set; }

        public string IncomeAccount { get; set; }

        public string ExpenseAccount { get; set; }

        public bool UseZoneBasedRates { get; set; }

        public List<PricingTierDto> PricingTiers { get; set; }
    }
}
