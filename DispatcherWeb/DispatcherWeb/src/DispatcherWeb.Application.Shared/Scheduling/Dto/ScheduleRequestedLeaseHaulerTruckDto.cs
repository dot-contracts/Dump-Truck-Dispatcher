using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleRequestedLeaseHaulerTruckDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderLineId { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public ScheduleTruckTrailerDto Trailer { get; set; }
        public int? DriverId { get; set; }
        public int? OfficeId { get; set; }
        public bool IsExternal { get; set; }
        public VehicleCategoryDto VehicleCategory { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
        public bool CanPullTrailer { get; set; }
        public LeaseHaulerRequestStatus Status { get; set; }
    }
}
