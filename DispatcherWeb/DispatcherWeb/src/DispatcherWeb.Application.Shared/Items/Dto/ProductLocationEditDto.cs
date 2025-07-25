using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class ProductLocationEditDto : EntityDto<int?>
    {
        public int ItemId { get; set; }

        public int? LocationId { get; set; }

        public decimal? Cost { get; set; }

        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        public string LocationName { get; set; }

        public ICollection<ProductLocationPriceDto> ProductLocationPrices { get; set; }
    }
}
