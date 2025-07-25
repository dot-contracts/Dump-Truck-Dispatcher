namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineIsCompleteInput
    {
        public int OrderLineId { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
    }
}
