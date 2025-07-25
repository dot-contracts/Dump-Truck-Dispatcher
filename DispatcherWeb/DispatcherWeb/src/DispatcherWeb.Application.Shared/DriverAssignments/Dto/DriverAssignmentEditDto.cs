using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class DriverAssignmentEditDto
    {
        public int Id { get; set; }

        public int TruckId { get; set; }

        public int? DriverId { get; set; }

        public DateTime? StartTime { get; set; }

        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
