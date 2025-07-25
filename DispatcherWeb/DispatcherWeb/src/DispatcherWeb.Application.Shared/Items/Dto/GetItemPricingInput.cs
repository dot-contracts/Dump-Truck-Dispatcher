using System.Collections.Generic;

namespace DispatcherWeb.Items.Dto
{
    public class GetItemPricingInput
    {
        public int? FreightItemId { get; set; }

        public int? MaterialItemId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public int? QuoteLineId { get; set; }

        public int? PricingTierId { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public List<int?> TruckCategoryIds { get; set; }

        public bool UseZoneBasedRates { get; set; }

        public bool CustomerIsCod { get; set; }

        public DesignationEnum Designation { get; set; }
    }
}
