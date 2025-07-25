using System;

namespace DispatcherWeb.TimeOffs.Dto
{
    public class RemoveDriverFromTrucksInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DriverId { get; set; }
    }
}
