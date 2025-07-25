using System;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class ScheduledStartTimeInfo
    {
        public DateTime Date { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public string TruckCode { get; set; }
        public bool HasDriverAssignment { get; set; }
        public DateTime? NextAssignmentDate { get; set; }
    }
}
