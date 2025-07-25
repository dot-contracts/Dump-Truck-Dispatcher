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
    public class HollisCsvExporter : CsvExporterBase, IHollisCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public HollisCsvExporter(
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
                        ("Customer", x => x.Invoice.Customer.Name),
                        ("InvoiceNo", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("Service Date", x => x.InvoiceLine.Ticket?.TicketDateTimeUtc?.ConvertTimeZoneTo(timezone).Date.ToString("d")),
                        ("Ticket No", x => x.InvoiceLine.TicketNumber),
                        ("Material", x => x.InvoiceLine.Item?.Name),
                        ("Truck", x => x.InvoiceLine.TruckCode),
                        ("Job No", x => x.InvoiceLine.JobNumber),
                        ("Zone", x => ""),
                        ("Quantity", x => x.InvoiceLine.Quantity.ToString()),
                        ("Notes", x => ""),
                        ("Description", x => x.InvoiceLine.Description),
                        ("Material Price", x => x.InvoiceLine.MaterialRate?.ToString()),
                        ("Haul Fee", x => x.InvoiceLine.FreightRate?.ToString()),
                        ("Mark Up", x => ""),
                        ("Rate", x => x.InvoiceLine.Rate?.ToString()),
                        ("Amount", x => x.InvoiceLine.MaterialExtendedAmount.ToString()),
                        ("Haul Amt", x => x.InvoiceLine.FreightExtendedAmount.ToString()),
                        ("Template", x => ""),
                        ("InvoiceDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("DueDate", x => x.Invoice.DueDate?.ToString("d"))
                    );
                });
        }
    }

}
