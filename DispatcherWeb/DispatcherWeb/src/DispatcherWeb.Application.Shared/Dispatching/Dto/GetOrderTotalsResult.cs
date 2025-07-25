namespace DispatcherWeb.Dispatching.Dto
{
    public class GetOrderTotalsResult
    {
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightActualAmount { get; set; }
        public decimal? MaterialActualAmount { get; set; }

        public bool ActualAmountExceedsOrderedQuantity => FreightActualAmount > FreightQuantity || MaterialActualAmount > MaterialQuantity;
        public bool ActualAmountReachedOrderedQuantity => FreightActualAmount >= FreightQuantity || MaterialActualAmount >= MaterialQuantity;
    }
}
