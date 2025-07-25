using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.PricingTiers.Dto
{
    public class PricingTierEditDto : EntityDto<int?>
    {
        [Required]
        [StringLength(EntityStringFieldLengths.PricingTier.Name)]
        public string Name { get; set; }

        public bool IsDefault { get; set; }
    }
}
