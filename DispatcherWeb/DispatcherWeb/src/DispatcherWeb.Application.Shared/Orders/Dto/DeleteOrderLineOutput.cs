using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class DeleteOrderLineOutput
    {
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
