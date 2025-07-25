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
    public class SageCsvExporter : CsvExporterBase, ISageCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public SageCsvExporter(
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
                        ("AccountNbr", x => x.Invoice.Customer.AccountNumber),
                        ("RefNumber", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("TxnDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("PONumber", x => x.Invoice.PONumber),
                        ("DueDate", x => x.Invoice.DueDate?.ToString("d")),
                        ("LineQty", x => x.InvoiceLine.Quantity.ToString()),
                        ("Ticket Date", x => x.InvoiceLine.Ticket?.TicketDateTimeUtc?.ConvertTimeZoneTo(timezone).Date.ToString("d")),
                        ("Ticket Nbr", x => x.InvoiceLine.TicketNumber),
                        ("Truck Number", x => x.InvoiceLine.TruckCode),
                        ("Job Description", x => x.InvoiceLine.Description),
                        ("Material", x => x.InvoiceLine.Item?.Name),
                        ("Rate", x => x.InvoiceLine.Rate?.ToString()),
                        ("Ticket Amt", x => x.InvoiceLine.Subtotal.ToString()),
                        ("Invoicing Co", x => x.Invoice.OfficeName)
                    );
                });
        }
    }
}
