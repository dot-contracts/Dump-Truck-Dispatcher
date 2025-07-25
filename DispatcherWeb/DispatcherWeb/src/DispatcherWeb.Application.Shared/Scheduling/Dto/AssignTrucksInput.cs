using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class AssignTrucksInput
    {
        public int OrderLineId { get; set; }
        public List<int> TruckIds { get; set; }
    }
}
