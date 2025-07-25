namespace DispatcherWeb.Invoices.Dto
{
    public class GetInvoicePrintOutInput
    {
        public int? InvoiceId { get; set; }
        public bool DebugLayout { get; set; }
        public int[] InvoiceIds { get; set; }
    }
}
