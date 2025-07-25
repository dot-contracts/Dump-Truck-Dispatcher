namespace DispatcherWeb.Scheduling.Dto
{
    public class ChangeOrderLineUtilizationInput
    {
        public int OrderLineId { get; set; }
        public decimal Utilization { get; set; }
    }
}
