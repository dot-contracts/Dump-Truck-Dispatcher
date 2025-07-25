using System;
using System.Collections.Generic;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class DriverAssignmentReportDto
    {
        public DateTime Date { get; set; }
        public List<DriverAssignmentReportItemDto> Items { get; set; }
        public Shift? Shift { get; set; }
        public string ShiftName { get; set; }
    }
}
