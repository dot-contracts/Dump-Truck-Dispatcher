using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.QuickbooksOnline.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public class QuickbooksOnlineCsvExporter : CsvExporterBase, IQuickbooksOnlineCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public QuickbooksOnlineCsvExporter(
            ISettingManager settingManager,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);

            return await CreateCsvFileAsync(
                filename + ".csv",
                () =>
                {
                    var flatData = invoiceList.SelectMany(x => x.InvoiceLines, (invoice, invoiceLine) => new
                    {
                        IsFirstRow = invoice.InvoiceLines.IndexOf(invoiceLine) == 0,
                        Invoice = invoice,
                        InvoiceLine = invoiceLine,
                    }).ToList();

                    AddHeaderAndData(flatData,
                        ("*InvoiceNo", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("*Customer", x => x.IsFirstRow ? x.Invoice.Customer.Name : ""),
                        ("*InvoiceDate", x => x.IsFirstRow ? x.Invoice.IssueDate?.ToString("d") : ""),
                        ("*DueDate", x => x.IsFirstRow ? x.Invoice.DueDate?.ToString("d") : ""),
                        ("Terms", x => x.IsFirstRow ? x.Invoice.Terms?.GetDisplayName() : ""),
                        ("Location", x => ""),
                        ("Memo", x => x.IsFirstRow ? x.Invoice.Message : ""),
                        ("Item(Product/Service)", x => x.InvoiceLine.Item?.Name),
                        ("ItemDescription", x => x.InvoiceLine.DescriptionAndTicketWithTruck),
                        ("ItemQuantity", x => x.InvoiceLine.Quantity.ToString()),
                        ("ItemRate", x => (x.InvoiceLine.Subtotal / x.InvoiceLine.Quantity).ToString()),
                        ("*ItemAmount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("Taxable", x => x.InvoiceLine.Tax > 0 ? "Y" : "N"),
                        ("TaxRate", x => x.IsFirstRow ? x.Invoice.TaxRate + "%" : ""),
                        ("Service Date", x => x.Invoice.IssueDate?.ToString("d"))
                    );
                });
        }
    }
}
