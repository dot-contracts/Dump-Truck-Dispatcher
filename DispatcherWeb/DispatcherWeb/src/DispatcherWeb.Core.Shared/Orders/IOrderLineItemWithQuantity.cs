namespace DispatcherWeb.Orders
{
    public interface IOrderLineItemWithQuantity
    {
        string MaterialItemName { get; }
        string FreightItemName { get; }
        decimal? MaterialQuantity { get; }
        decimal? FreightQuantity { get; }
        string MaterialUomName { get; }
        string FreightUomName { get; }
    }
}
