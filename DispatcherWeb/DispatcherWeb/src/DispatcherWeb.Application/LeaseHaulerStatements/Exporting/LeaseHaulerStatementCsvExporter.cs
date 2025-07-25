using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.LeaseHaulerStatements.Dto;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public class LeaseHaulerStatementCsvExporter : CsvExporterBase, ILeaseHaulerStatementCsvExporter
    {
        private readonly IFeatureChecker _featureChecker;

        public LeaseHaulerStatementCsvExporter(
            ITempFileCacheManager tempFileCacheManager,
            IFeatureChecker featureChecker
        ) : base(tempFileCacheManager)
        {
            _featureChecker = featureChecker;
        }

        public async Task<FileDto> ExportToFileAsync(LeaseHaulerStatementReportDto data)
        {

            return await CreateCsvFileAsync(
                data.FileName, async () =>
                {
                    await FillCsvFile(data);
                }
            );
        }

        public async Task<FileBytesDto> ExportToFileBytes(LeaseHaulerStatementReportDto data)
        {
            return await CreateCsvFileBytesAsync(
                data.FileName, async () =>
                {
                    await FillCsvFile(data);
                }
            );
        }

        private async Task FillCsvFile(LeaseHaulerStatementReportDto data)
        {
            var showFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            AddHeader(
                data.Id.HasValue ? $"Statement Id: {data.Id}" : null,
                $"Statement Date: {data.StatementDate:d}",
                $"Start Date: {data.StartDate:d}",
                $"End Date: {data.EndDate:d}"
            );

            AddHeaderAndData(
                data.Tickets,
                ("Order Date", x => x.OrderDate?.ToString("d")),
                ("Shift", x => x.ShiftName),
                ("Customer", x => x.CustomerName),
                (separateItems ? "Freight Item" : "Product / Service", x => x.FreightItemName),
                (separateItems ? "Material Item" : null, x => x.MaterialItemName),
                ("Ticket #", x => x.TicketNumber),
                ("Ticket Date Time", x => x.TicketDateTime?.ToString("g")),
                ("Carrier", x => x.CarrierName),
                ("Truck", x => x.TruckCode),
                ("Driver", x => x.DriverName),
                ("Load At", x => x.LoadAtName),
                ("Deliver To", x => x.DeliverToName),
                ("Freight UOM", x => x.FreightUomName),
                ("Material UOM", x => x.MaterialUomName),
                ("Quantity", x => x.Quantity.ToString()),
                ("Rate", x => x.LeaseHaulerRate?.ToString()),
                ("BrokerFee", x => x.BrokerFee.ToString()),
                (showFuelSurcharge ? "Fuel" : null, showFuelSurcharge ? x => x.FuelSurcharge.ToString("N2") : null),
                ("ExtendedAmount", x => x.ExtendedAmount.ToString())
            );
        }
    }
}
