namespace DispatcherWeb.Dispatching.Dto
{
    public class TruckDispatchClockInInfoDto
    {
        public long? UserId { get; set; }
        public bool IsClockedIn { get; set; }
        public bool HasClockedInToday { get; set; }
    }
}
