namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderLineTaxTotalDetails : IOrderLineTaxDetails
    {
        decimal Tax { get; set; }

        decimal Subtotal { get; set; }

        decimal TotalAmount { get; set; }
    }
}
