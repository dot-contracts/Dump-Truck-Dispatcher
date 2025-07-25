using System;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class EmployeeTimeDto
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public decimal? ManualHourAmount { get; set; }
        public string TimeClassificationName { get; set; }
        public decimal ElapsedHours => ManualHourAmount.HasValue ? ManualHourAmount.Value : EndDateTime.HasValue ? (decimal)(EndDateTime.Value - StartDateTime).TotalHours : 0M;
        public int? ElapsedHoursSort { get; set; }
    }
}
