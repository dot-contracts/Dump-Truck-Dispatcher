namespace DispatcherWeb.Scheduling.Dto
{
    public class HasDispatchesResult
    {
        public string TruckCode { get; set; }
        public bool Unacknowledged { get; set; }
        public bool AcknowledgedOrLoaded { get; set; }
    }
}
