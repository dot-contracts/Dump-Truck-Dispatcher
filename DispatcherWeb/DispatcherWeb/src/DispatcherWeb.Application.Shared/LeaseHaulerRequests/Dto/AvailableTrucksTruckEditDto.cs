namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class AvailableTrucksTruckEditDto
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public bool IsTruckInUse { get; set; }
        public bool IsDriverInUse { get; set; }
    }
}
