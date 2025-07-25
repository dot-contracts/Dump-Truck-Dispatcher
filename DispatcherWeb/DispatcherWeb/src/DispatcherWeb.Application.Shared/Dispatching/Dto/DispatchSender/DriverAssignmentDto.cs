using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class DriverAssignmentDto
    {
        public int DriverId { get; set; }
        public int TruckId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public DateTime? StartTimeUtc { get; set; }
    }
}
