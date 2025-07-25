using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class DeleteOrderLineTruckInput
    {
        public int OrderLineTruckId { get; set; }

        [Obsolete("We'll query OrderLineId from the provided OrderLineTruckId to be safer. The property remains only for audit logs.")]
        public int OrderLineId { get; set; }

        public bool MarkAsDone { get; set; }
    }
}
