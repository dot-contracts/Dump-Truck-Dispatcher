using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetTruckOrdersInput
    {
        public int TruckId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public Shift? Shift { get; set; }
    }
}
