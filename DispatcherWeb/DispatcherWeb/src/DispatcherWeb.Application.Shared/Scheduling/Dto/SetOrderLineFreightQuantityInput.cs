namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineFreightQuantityInput
    {
        public int OrderLineId { get; set; }
        public decimal? FreightQuantity { get; set; }
    }
}
