namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderLineTruckToChangeDriverDto
    {
        public int OrderLineTruckId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public bool HasTicketsOrLoads { get; set; }
        public bool IsExternal { get; set; }
        public int? LeaseHaulerId { get; set; }
    }
}
