using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class ProductLocationDto : EntityDto
    {
        public string LocationName { get; set; }

        public string UOM { get; set; }

        public decimal? Cost { get; set; }

        public List<ProductLocationPriceDto> ProductLocationPrices { get; set; }
    }
}
