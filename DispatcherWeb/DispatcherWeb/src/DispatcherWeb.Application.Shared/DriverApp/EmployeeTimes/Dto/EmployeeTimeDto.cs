using System;

namespace DispatcherWeb.DriverApp.EmployeeTimes.Dto
{
    public class EmployeeTimeDto : EmployeeTimeEditDto
    {
        public DateTime? LastModifiedDateTime { get; set; }
        public bool IsEditable { get; set; }
        public bool IsImported { get; set; }
    }
}
