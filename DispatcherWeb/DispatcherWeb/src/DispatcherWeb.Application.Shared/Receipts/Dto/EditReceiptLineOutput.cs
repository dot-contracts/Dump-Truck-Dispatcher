using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Receipts.Dto
{
    public class EditReceiptLineOutput
    {
        public int ReceiptLineId { get; set; }
        public IOrderTaxDetails OrderTaxDetails { get; set; }
    }
}
