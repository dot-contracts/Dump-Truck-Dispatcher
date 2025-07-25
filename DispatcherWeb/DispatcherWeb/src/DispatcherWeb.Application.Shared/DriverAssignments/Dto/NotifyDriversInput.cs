using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class NotifyDriversInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
    }
}
