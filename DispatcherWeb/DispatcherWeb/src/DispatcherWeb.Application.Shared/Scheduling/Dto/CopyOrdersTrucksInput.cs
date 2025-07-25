namespace DispatcherWeb.Scheduling.Dto
{
    public class CopyOrdersTrucksInput
    {
        public int OriginalOrderId { get; set; }
        public int[] NewOrderIds { get; set; }
        public int? OrderLineId { get; set; }
        public bool ProceedOnConflict { get; set; }

    }
}
