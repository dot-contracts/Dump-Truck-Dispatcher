using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class HasOrderLineTrucksInput
    {
        public int? DriverId { get; set; }
        public int? TruckId { get; set; }
        public int? TrailerId { get; set; }
        public bool ForceTrailerIdFilter { get; set; }

        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
    }
}
