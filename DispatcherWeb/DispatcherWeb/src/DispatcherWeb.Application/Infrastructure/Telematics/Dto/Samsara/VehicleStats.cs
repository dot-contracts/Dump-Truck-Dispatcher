namespace DispatcherWeb.Infrastructure.Telematics.Dto.Samsara
{
    public class VehicleStats
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public VehicleStat<long> ObdOdometerMeters { get; set; }
        public VehicleStat<long> GpsOdometerMeters { get; set; }
        public VehicleStat<long> ObdEngineSeconds { get; set; }
    }
}
