namespace DispatcherWeb.Trucks.Dto
{
    public class SaveCurrentMileageInput
    {
        public int TruckId { get; set; }
        public decimal CurrentMileage { get; set; }
        public decimal CurrentHours { get; set; }
    }
}
