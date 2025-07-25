namespace DispatcherWeb.Invoices.Dto
{
    public class EmailInvoicePrintOutDto : EmailInvoicePrintOutBaseDto
    {
        public int InvoiceId { get; set; }
        public string To { get; set; }
    }
}
