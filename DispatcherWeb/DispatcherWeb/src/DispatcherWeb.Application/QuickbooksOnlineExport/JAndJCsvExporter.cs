using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.QuickbooksOnline.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public class JAndJCsvExporter : CsvExporterBase, IJAndJCsvExporter
    {
        private static class TaxTypes
        {
            public const string Freight = "27";
            public const string Material = "0";
        }

        private readonly ISettingManager _settingManager;

        public JAndJCsvExporter(
            ISettingManager settingManager,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);

            return await CreateCsvFileAsync(
                filename + ".csv",
                () =>
                {
                    var flatData = invoiceList.SelectMany(x => x.InvoiceLines, (invoice, invoiceLine) => new
                    {
                        Invoice = invoice,
                        InvoiceLine = invoiceLine,
                    }).ToList();

                    AddHeaderAndData(flatData,
                        ("Customer Id", x => x.Invoice.Customer.AccountNumber),
                        ("Invoice #", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("Date", x => x.Invoice.IssueDate?.ToString("d")),
                        ("Ship to Name", x => x.Invoice.Customer.Name),
                        ("Ship to Address Line One", x => x.Invoice.Customer.BillingAddress.Address1),
                        ("Ship to Address Line Two", x => x.Invoice.Customer.BillingAddress.Address2),
                        ("Ship to Address City", x => x.Invoice.Customer.BillingAddress.City),
                        ("Ship to State", x => x.Invoice.Customer.BillingAddress.State),
                        ("Ship to Zipcode", x => x.Invoice.Customer.BillingAddress.ZipCode),
                        ("Customer PO", x => x.Invoice.PONumber),
                        ("Ship Date", x => x.InvoiceLine.DeliveryDateTime?.ToString("d")),
                        ("Due Date", x => x.Invoice.DueDate?.ToString("d")),
                        ("Accounts Receivable Account", x => x.InvoiceLine.Item?.IncomeAccount),
                        ("Invoice Note", x => x.Invoice.Message),
                        ("Number of Distributions", x => "1"),
                        ("Quantity", x => x.InvoiceLine.Quantity.ToString()),
                        ("Item Id", x => x.InvoiceLine.Item?.Name),
                        ("Serial Number", x => x.InvoiceLine.LineNumber.ToString()),
                        ("U/M ID", x => x.InvoiceLine.Ticket?.TicketUomName),
                        ("Unit Price", x => ((x.InvoiceLine.FreightRate ?? 0) + (x.InvoiceLine.MaterialRate ?? 0)).ToString()),
                        ("FreightUnitPrice", x => x.InvoiceLine.FreightRate?.ToString()),
                        ("MaterialUnitPrice", x => x.InvoiceLine.MaterialRate?.ToString()),
                        ("Tax Type", x => x.InvoiceLine.MaterialExtendedAmount > 0 ? TaxTypes.Material : TaxTypes.Freight),
                        ("Amount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("FreightAmount", x => x.InvoiceLine.FreightExtendedAmount.ToString()),
                        ("MaterialAmount", x => x.InvoiceLine.MaterialExtendedAmount.ToString()),
                        ("Sales Tax Agency", x => x.Invoice.TaxName),
                        ("Voided By Transaction", x => "0"),
                        ("Recur Number", x => "0"),
                        ("Recur Frequency", x => "0")
                    );
                });
        }
    }
}
