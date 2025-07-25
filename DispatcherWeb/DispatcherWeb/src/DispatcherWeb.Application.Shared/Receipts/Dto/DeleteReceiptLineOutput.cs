using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Receipts.Dto
{
    public class DeleteReceiptLineOutput
    {
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
