using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class SetDriverForTruckInput
    {
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? LeaseHaulerId { get; set; }
        public DateTime Date { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
        public bool CreateNewDriverAssignment { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
    }
}
