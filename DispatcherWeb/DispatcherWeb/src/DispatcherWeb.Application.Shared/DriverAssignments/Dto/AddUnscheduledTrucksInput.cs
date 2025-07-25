using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class AddUnscheduledTrucksInput
    {
        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public DateTime? DefaultStartTime { get; set; }
    }
}
