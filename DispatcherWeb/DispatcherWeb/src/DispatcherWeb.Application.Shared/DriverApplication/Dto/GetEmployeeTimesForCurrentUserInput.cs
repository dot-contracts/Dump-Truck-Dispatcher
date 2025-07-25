using System;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class GetEmployeeTimesForCurrentUserInput
    {
        public Guid? DriverGuid { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? UpdatedAfterDateTime { get; set; }
    }
}
