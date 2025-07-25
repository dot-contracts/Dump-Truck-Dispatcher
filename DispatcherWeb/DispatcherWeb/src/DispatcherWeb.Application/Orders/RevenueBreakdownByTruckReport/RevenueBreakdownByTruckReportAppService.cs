using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using DispatcherWeb.Orders.RevenueBreakdownReport;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    [AbpAuthorize(AppPermissions.Pages_Reports_RevenueBreakdownByTruck)]
    public class RevenueBreakdownByTruckReportAppService : ReportAppServiceBase<RevenueBreakdownByTruckReportInput>
    {
        private readonly IRevenueBreakdownTimeCalculator _revenueBreakdownTimeCalculator;
        private readonly IRevenueBreakdownByTruckReportByTicketsDataService _reportDataService;

        public RevenueBreakdownByTruckReportAppService(
            IAttachmentHelper attachmentHelper,
            IRevenueBreakdownTimeCalculator revenueBreakdownTimeCalculator,
            IRevenueBreakdownByTruckReportByTicketsDataService reportDataService
        ) : base(attachmentHelper)
        {
            _revenueBreakdownTimeCalculator = revenueBreakdownTimeCalculator;
            _reportDataService = reportDataService;
        }

        protected override string ReportPermission => AppPermissions.Pages_Reports_RevenueBreakdownByTruck;
        protected override string ReportFileName => "RevenueBreakdownByTruck";
        protected override Task<string> GetReportFilename(string extension, RevenueBreakdownByTruckReportInput input)
        {
            return Task.FromResult($"{ReportFileName}_{input.DeliveryDateBegin:yyyyMMdd}to{input.DeliveryDateEnd:yyyyMMdd}.{extension}");
        }

        protected override void InitPdfReport(PdfReport report)
        {
        }

        protected override Task<bool> CreatePdfReport(PdfReport report, RevenueBreakdownByTruckReportInput input)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> CreateCsvReport(CsvReport report, RevenueBreakdownByTruckReportInput input)
        {
            return CreateReport(report, input, () => new RevenueBreakdownByTruckTableCsv(report.CsvWriter));
        }

        private async Task<bool> CreateReport(
            IReport report,
            RevenueBreakdownByTruckReportInput input,
            Func<IRevenueBreakdownByTruckTable> createRevenueBreakdownTable
        )
        {
            report.AddReportHeader($"Revenue Breakdown Report for {input.DeliveryDateBegin:d} - {input.DeliveryDateEnd:d}");

            var showFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);

            var revenueBreakdownItems = await GetRevenueBreakdownItems(input);
            if (revenueBreakdownItems.Count == 0)
            {
                return false;
            }

            var revenueBreakdownTable = createRevenueBreakdownTable();

            revenueBreakdownTable.AddColumnHeaders(
                "Ticket Date",
                await SettingManager.UseShifts() ? "Shift" : null,
                "Truck",
                "Material Revenue",
                "Freight Revenue",
                showFuelSurcharge ? "Fuel Surcharge" : null,
                "Total Revenue",
                "Driver Time",
                "Revenue/hr"
            );
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

            foreach (var item in revenueBreakdownItems)
            {
                revenueBreakdownTable.AddRow(
                    item.TicketDate?.ToString("d") ?? "",
                    item.Shift.HasValue && shiftDictionary.ContainsKey(item.Shift.Value) ? shiftDictionary[item.Shift.Value] : null,
                    item.Truck,
                    item.MaterialRevenue.ToString("C", currencyCulture),
                    item.FreightRevenue.ToString("C", currencyCulture),
                    showFuelSurcharge ? item.FuelSurcharge.ToString("C", currencyCulture) : null,
                    item.TotalRevenue.ToString("C", currencyCulture),
                    item.DriverTime.ToString("g"),
                    item.RevenuePerHour?.ToString("C", currencyCulture) ?? ""
                );
            }

            return true;
        }

        private async Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input)
        {
            var items = await _reportDataService.GetRevenueBreakdownItems(input);

            await _revenueBreakdownTimeCalculator.FillDriversTimeForTrucks(items, input);

            return items;
        }

    }
}
