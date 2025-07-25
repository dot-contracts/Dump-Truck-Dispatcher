using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class EditOrderLineOutput
    {
        public int OrderLineId { get; set; }
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
