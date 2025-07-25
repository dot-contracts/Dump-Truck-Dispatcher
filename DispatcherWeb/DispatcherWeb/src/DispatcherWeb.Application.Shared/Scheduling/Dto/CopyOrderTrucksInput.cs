namespace DispatcherWeb.Scheduling.Dto
{
    public class CopyOrderTrucksInput
    {
        public int OriginalOrderId { get; set; }
        public int NewOrderId { get; set; }
        public int? OrderLineId { get; set; }
        public bool ProceedOnConflict { get; set; }
    }
}
