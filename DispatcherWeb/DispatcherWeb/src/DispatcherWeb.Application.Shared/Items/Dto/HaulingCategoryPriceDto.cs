using Abp.Application.Services.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class HaulingCategoryPriceDto : EntityDto
    {
        public int HaulingCategoryId { get; set; }

        public int PricingTierId { get; set; }

        public string PricingTierName { get; set; }

        public decimal? PricePerUnit { get; set; }
    }
}
