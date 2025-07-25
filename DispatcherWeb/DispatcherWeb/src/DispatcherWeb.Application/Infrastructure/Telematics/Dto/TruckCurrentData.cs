namespace DispatcherWeb.Infrastructure.Telematics.Dto
{
    public class TruckCurrentData
    {
        public string TruckCodeOrUniqueId { get; set; }
        public double CurrentMileage { get; set; }
        public double CurrentHours { get; set; }
    }
}
