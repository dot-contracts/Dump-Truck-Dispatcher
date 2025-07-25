namespace DispatcherWeb.Dispatching.Dto
{
    public class TruckDriverDto
    {
        public int OrderLineId { get; set; }
        public string TruckCode { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int TruckId { get; set; }
        public bool HasPhone { get; set; }
        public OrderNotifyPreferredFormat DriverPreferredNotifyFormat { get; set; }
        public int OrderLineTruckId { get; set; }
    }
}
