namespace DispatcherWeb.Invoices.Dto
{
    public class ValidateInvoiceStatusChangeInput
    {
        public int[] Ids { get; set; }

        public InvoiceStatus Status { get; set; }
    }
}
