using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class ThereAreOpenDispatchesForDriverOnDateInput
    {
        public int DriverId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
