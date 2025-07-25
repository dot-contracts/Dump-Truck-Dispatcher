using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class HaulingCategoryEditDto : EntityDto<int?>
    {
        public int ItemId { get; set; }

        public int? TruckCategoryId { get; set; }

        public string TruckCategoryName { get; set; }

        public int UnitOfMeasureId { get; set; }

        public string UnitOfMeasureName { get; set; }

        public decimal MinimumBillableUnits { get; set; }

        public decimal LeaseHaulerRate { get; set; }

        public ICollection<HaulingCategoryPriceDto> HaulingCategoryPrices { get; set; }
    }
}

