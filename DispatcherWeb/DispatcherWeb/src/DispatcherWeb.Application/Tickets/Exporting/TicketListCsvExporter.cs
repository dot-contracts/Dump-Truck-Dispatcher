using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets.Exporting
{
    public class TicketListCsvExporter : CsvExporterBase, ITicketListCsvExporter
    {
        private readonly IFeatureChecker _featureChecker;

        public TicketListCsvExporter(
            ITempFileCacheManager tempFileCacheManager,
            IFeatureChecker featureChecker
        )
            : base(tempFileCacheManager)
        {
            _featureChecker = featureChecker;
        }

        public async Task<FileDto> ExportToFileAsync(List<TicketListViewDto> ticketDtos, string fileName, bool hideColumnsForInvoiceExport)
        {
            var showFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            var showFreightRateToPayDriverColumn = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate);
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var basePayOnHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.BasePayOnHourlyJobRate);
            var useDriverSpecificHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate);
            var allowLoadCount = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs);
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            return await CreateCsvFileAsync(
                fileName,
                () =>
                {
                    AddHeaderAndData(
                        ticketDtos,
                        ("Ticket Id", x => x.Id.ToString()),
                        ("Ticket Date", x => x.Date?.ToString("g")),
                        ("Order Date", x => x.OrderDate?.ToString("d")),
                        ("Shift", x => x.Shift),
                        ("Office", x => x.Office),
                        ("Customer", x => x.CustomerName),
                        ("Customer Number", x => x.CustomerNumber),
                        ("Quote Name", x => x.QuoteName),
                        ("Job Nbr", x => x.JobNumber),
                        (separateItems ? "Freight Item" : "Item", x => x.FreightItemName),
                        (separateItems ? "Material Item" : null, x => x.MaterialItemName),
                        ("Ticket #", x => x.TicketNumber),
                        ("Carrier", x => x.Carrier),
                        ("Invoice Number", x => x.InvoiceNumber?.ToString()),
                        ("Receipt", x => x.ReceiptId?.ToString()),
                        ("Truck", x => x.Truck),
                        ("Truck Office", x => x.TruckOffice),
                        ("Trailer", x => x.Trailer),
                        ("Driver", x => x.DriverName),
                        ("Employee Id", x => x.EmployeeId),
                        ("Driver Office", x => x.DriverOffice),
                        ("Load At", x => x.LoadAtName),
                        ("Deliver To", x => x.DeliverToName),
                        ("Freight UOM", x => x.FreightUomName),
                        ("Material UOM", x => x.MaterialUomName),
                        ("Freight Quantity", x => x.FreightQuantity?.ToString("N")),
                        ("Material Quantity", x => x.MaterialQuantity?.ToString("N")),
                        (hideColumnsForInvoiceExport ? null : "Revenue", x => x.Revenue.ToString("N")),
                        (!hideColumnsForInvoiceExport && allowLoadCount ? "Load Count" : null, allowLoadCount ? x => x.LoadCount?.ToString() : null),
                        ("Freight Rate", x => x.FreightRate?.ToString("N4")),
                        (!hideColumnsForInvoiceExport && showFreightRateToPayDriverColumn ? "Freight Rate to Pay Driver" : null, x => x.FreightRateToPayDrivers?.ToString("N4")),
                        (!hideColumnsForInvoiceExport && allowProductionPay ? "Driver pay %" : null, allowProductionPay ? x => x.DriverPayPercent?.ToString("N") : null),
                        ("Material Rate", x => x.MaterialRate?.ToString("N4")),
                        (hideColumnsForInvoiceExport ? null : "Freight Amount", x => x.FreightAmount?.ToString("N")),
                        (hideColumnsForInvoiceExport ? null : "Material Amount", x => x.MaterialAmount?.ToString("N")),
                        (!hideColumnsForInvoiceExport && separateItems ? "MaterialCostRate" : null, x => x.MaterialCostRate?.ToString("N")),
                        (!hideColumnsForInvoiceExport && separateItems ? "MaterialCost" : null, x => x.MaterialCost?.ToString("N")),
                        (!hideColumnsForInvoiceExport && allowProductionPay ? "Driver Pay" : null, allowProductionPay ? x => x.DriverPay?.ToString("N") : null),
                        (!hideColumnsForInvoiceExport && basePayOnHourlyJobRate ? "Driver Pay Time Code" : null, x => x.DriverPayTimeClassificationName),
                        (!hideColumnsForInvoiceExport && basePayOnHourlyJobRate ? "Use driver specific rate" : null, x => useDriverSpecificHourlyJobRate.ToYesNoString() ?? ""),
                        (!hideColumnsForInvoiceExport && basePayOnHourlyJobRate ? "Driver Pay Rate" : null, x => (useDriverSpecificHourlyJobRate ? x.DriverSpecificHourlyRate : x.HourlyDriverPayRate)?.ToString("N")),
                        (!hideColumnsForInvoiceExport && showFuelSurcharge ? "Fuel Surcharge" : null, showFuelSurcharge ? x => x.FuelSurcharge?.ToString("N") : null),
                        (hideColumnsForInvoiceExport ? null : "Price Override", x => x.PriceOverride?.ToString("N2")),
                        (hideColumnsForInvoiceExport ? null : "Billed", x => x.IsBilled.ToYesNoString()),
                        (hideColumnsForInvoiceExport ? null : "Imported", x => x.IsImported.ToYesNoString()),
                        (hideColumnsForInvoiceExport ? null : "Verified", x => x.IsVerified.ToYesNoString()),
                        (hideColumnsForInvoiceExport ? null : "Production pay", x => x.ProductionPay?.ToYesNoString() ?? ""),
                        (hideColumnsForInvoiceExport ? null : "Statement", x => x.PayStatementId?.ToString()),
                        (hideColumnsForInvoiceExport ? null : "Order Note", x => x.OrderNote),
                        (hideColumnsForInvoiceExport ? null : "Driver Note", x => x.DriverNote),
                        ("PO Number", x => x.PONumber),
                        (hideColumnsForInvoiceExport ? null : "Order Id", x => x.OrderId?.ToString()),
                        (hideColumnsForInvoiceExport ? null : "Lease Hauler Rate", x => (x.CarrierId.HasValue ? x.LeaseHaulerRate : 0)?.ToString("N")),
                        (hideColumnsForInvoiceExport ? null : "Lease Hauler Cost", x => (x.CarrierId.HasValue ? x.LeaseHaulerCost : 0)?.ToString("N")),
                        (hideColumnsForInvoiceExport ? null : "Tax Name", x => x.SalesTaxEntityName),
                        (hideColumnsForInvoiceExport ? null : "Tax Rate", x => x.SalesTaxRate?.ToString("N"))
                    );
                }
            );
        }
    }
}
