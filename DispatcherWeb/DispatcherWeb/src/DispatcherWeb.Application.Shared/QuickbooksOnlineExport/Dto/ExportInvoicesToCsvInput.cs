using DispatcherWeb.Invoices.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport.Dto
{
    public class ExportInvoicesToCsvInput : GetInvoicesInput
    {
        public bool IncludeExportedInvoices { get; set; }
    }
}
