namespace DispatcherWeb.Trucks.Dto
{
    public class GetTruckCodeByDefaultDriverInput
    {
        public int? ExceptTruckId { get; set; }
        public int DefaultDriverId { get; set; }
    }
}
