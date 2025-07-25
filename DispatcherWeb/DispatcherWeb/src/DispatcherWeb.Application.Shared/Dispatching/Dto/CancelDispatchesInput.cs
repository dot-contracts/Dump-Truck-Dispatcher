using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelDispatchesInput
    {
        public int? OrderLineId { get; set; }
        public int? TruckId { get; set; }
        public int[] TruckIds { get; set; }
        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }
    }
}
