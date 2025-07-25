using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineTimeInput
    {
        public int OrderLineId { get; set; }
        public DateTime? Time { get; set; }
    }
}
