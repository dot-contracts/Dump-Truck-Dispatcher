using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class SetNoDriverForTruckInput
    {
        public int TruckId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Shift? Shift { get; set; }
    }
}
