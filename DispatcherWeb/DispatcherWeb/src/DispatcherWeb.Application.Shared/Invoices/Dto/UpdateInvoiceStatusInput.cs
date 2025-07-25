namespace DispatcherWeb.Invoices.Dto
{
    public class UpdateInvoiceStatusInput
    {
        public int Id { get; set; }

        public InvoiceStatus Status { get; set; }
    }
}
