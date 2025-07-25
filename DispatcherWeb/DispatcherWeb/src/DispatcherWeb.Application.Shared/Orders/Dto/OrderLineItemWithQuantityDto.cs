namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineItemWithQuantityDto : IOrderLineItemWithQuantity
    {
        public string MaterialItemName { get; set; }
        public string FreightItemName { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
    }
}
