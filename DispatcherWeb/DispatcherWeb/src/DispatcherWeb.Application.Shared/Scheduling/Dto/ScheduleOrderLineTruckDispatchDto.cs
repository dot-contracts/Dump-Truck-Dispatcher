namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleOrderLineTruckDispatchDto
    {
        public int Id { get; set; }

        public DispatchStatus Status { get; set; }

        public bool IsMultipleLoads { get; set; }
    }
}
