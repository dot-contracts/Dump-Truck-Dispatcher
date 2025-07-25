using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class HaulingCategoryDto : EntityDto
    {
        public string TruckCategoryName { get; set; }

        public string UOM { get; set; }

        public decimal? MinimumBillableUnits { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public List<HaulingCategoryPriceDto> HaulingCategoryPrices { get; set; }
    }
}
