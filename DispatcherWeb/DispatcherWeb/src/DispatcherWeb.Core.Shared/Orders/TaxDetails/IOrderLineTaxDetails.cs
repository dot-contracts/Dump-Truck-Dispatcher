namespace DispatcherWeb.Orders.TaxDetails
{
    public interface IOrderLineTaxDetails
    {
        bool? IsTaxable { get; }

        bool? IsMaterialTaxable { get; }

        bool? IsFreightTaxable { get; }

        decimal MaterialPrice { get; }

        decimal FreightPrice { get; }
    }
}
