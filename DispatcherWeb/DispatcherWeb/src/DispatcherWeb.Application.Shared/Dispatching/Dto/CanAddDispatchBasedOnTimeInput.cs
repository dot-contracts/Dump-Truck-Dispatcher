using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CanAddDispatchBasedOnTimeInput
    {
        public int OrderLineId { get; set; }
        public List<int> OrderLineTruckIds { get; set; }
    }
}
