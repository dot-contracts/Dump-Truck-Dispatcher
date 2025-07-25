using DispatcherWeb.Invoices.Dto;

namespace DispatcherWeb.QuickbooksDesktop.Dto
{
    public class ExportInvoicesToIIFInput : GetInvoicesInput
    {
        public bool IncludeExportedInvoices { get; set; }
    }
}
