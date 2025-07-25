namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineMaterialQuantityInput
    {
        public int OrderLineId { get; set; }
        public decimal? MaterialQuantity { get; set; }
    }
}
