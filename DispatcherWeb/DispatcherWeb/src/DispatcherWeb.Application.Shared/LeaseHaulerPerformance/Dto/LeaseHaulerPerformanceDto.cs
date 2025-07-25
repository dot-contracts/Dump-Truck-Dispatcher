namespace DispatcherWeb.LeaseHaulerPerformance.Dto
{
    public class LeaseHaulerPerformanceDto
    {
        public int LeaseHaulerId { get; set; }

        public string LeaseHaulerName { get; set; }

        public int Completed { get; set; }

        public int Canceled { get; set; }

        public int Total { get; set; }

        public decimal PercentComplete { get; set; }
    }
}
