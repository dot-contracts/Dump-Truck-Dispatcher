using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class DriverAssignmentDto : DriverAssignmentLiteDto
    {
        public DateTime? FirstTimeOnJob { get; set; }
        public string LoadAtName { get; set; }
    }
}
