using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetScheduleInput : IGetScheduleInput
    {
        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? LeaseHaulerId { get; set; }
    }
}
