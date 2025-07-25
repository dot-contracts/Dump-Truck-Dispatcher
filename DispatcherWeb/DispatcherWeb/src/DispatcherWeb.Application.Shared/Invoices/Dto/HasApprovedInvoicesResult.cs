namespace DispatcherWeb.Invoices.Dto
{
    public class HasApprovedInvoicesResult
    {
        public bool HasApprovedInvoicesToPrint { get; set; }
        public bool HasApprovedInvoicesToEmail { get; set; }
    }
}
