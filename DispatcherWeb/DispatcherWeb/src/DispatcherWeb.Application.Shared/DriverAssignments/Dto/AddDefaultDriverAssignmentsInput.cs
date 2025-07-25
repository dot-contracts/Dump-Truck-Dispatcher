using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class AddDefaultDriverAssignmentsInput
    {
        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
    }
}
