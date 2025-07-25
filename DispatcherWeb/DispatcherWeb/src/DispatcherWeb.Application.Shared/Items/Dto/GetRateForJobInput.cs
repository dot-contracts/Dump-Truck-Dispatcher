using System.Collections.Generic;

namespace DispatcherWeb.Items.Dto
{
    public class GetRateForJobInput
    {
        public int PricingTierId { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public int ItemId { get; set; }

        public int? UomId { get; set; }

        public List<int?> TruckCategoryIds { get; set; }
    }
}
