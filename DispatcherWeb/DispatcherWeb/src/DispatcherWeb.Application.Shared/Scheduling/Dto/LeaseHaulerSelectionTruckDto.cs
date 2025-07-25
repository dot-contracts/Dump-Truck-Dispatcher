namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerSelectionTruckDto
    {
        public int LeaseHaulerId { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DefaultDriverId { get; set; }
    }
}
