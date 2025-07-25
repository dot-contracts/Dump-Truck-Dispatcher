using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class IsTrailerAssignedToAnotherTruckInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public int TrailerId { get; set; }
        public int TruckId { get; set; }
    }
}
