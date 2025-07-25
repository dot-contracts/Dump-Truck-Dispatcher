using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportEmployeeTimeDto
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public string TimeClassificationName { get; set; }
    }
}
