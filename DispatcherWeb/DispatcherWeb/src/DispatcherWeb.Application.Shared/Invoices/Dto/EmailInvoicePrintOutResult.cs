namespace DispatcherWeb.Invoices.Dto
{
    public class EmailInvoicePrintOutResult
    {
        public bool Success { get; set; }
        public bool FromEmailAddressIsNotVerifiedError { get; set; }
        public bool SomeEmailsWereNotSentError { get; set; }
    }
}
