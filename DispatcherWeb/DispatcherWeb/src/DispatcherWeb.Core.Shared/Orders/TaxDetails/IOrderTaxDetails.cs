namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderTaxDetails
    {
        int Id { get; set; }
        decimal SalesTaxRate { get; set; }
        decimal SalesTax { get; set; }
        decimal CODTotal { get; set; }
        decimal FreightTotal { get; set; }
        decimal MaterialTotal { get; set; }
    }
}
