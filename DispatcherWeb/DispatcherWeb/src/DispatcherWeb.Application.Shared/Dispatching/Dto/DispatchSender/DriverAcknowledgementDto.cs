namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class DriverAcknowledgementDto
    {
        public int DriverId { get; set; }
        public bool HasAcknowledgedDispatchToday { get; set; }
    }
}
