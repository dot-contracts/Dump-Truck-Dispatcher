using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport;
using DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.WorkOrders
{
    [AbpAuthorize(AppPermissions.Pages_Reports_VehicleWorkOrderCost)]
    public class VehicleWorkOrderCostReportAppService : ReportAppServiceBase<VehicleWorkOrderCostReportInput>
    {
        private readonly IRepository<WorkOrder> _workOrderRepository;

        public VehicleWorkOrderCostReportAppService(
            IAttachmentHelper attachmentHelper,
            IRepository<WorkOrder> workOrderRepository
        ) : base(attachmentHelper)
        {
            _workOrderRepository = workOrderRepository;
        }

        protected override string ReportPermission => AppPermissions.Pages_Reports_VehicleWorkOrderCost;
        protected override string ReportFileName => "VehicleWorkOrderCost";
        protected override Task<string> GetReportFilename(string extension, VehicleWorkOrderCostReportInput input)
        {
            return Task.FromResult($"{ReportFileName}.{extension}");
        }

        protected override void InitPdfReport(PdfReport report)
        {
        }

        protected override Task<bool> CreatePdfReport(PdfReport report, VehicleWorkOrderCostReportInput input)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> CreateCsvReport(CsvReport report, VehicleWorkOrderCostReportInput input)
        {
            return CreateReport(report, input, () => new VehicleWorkOrderCostTableCsv(report.CsvWriter));
        }

        private async Task<bool> CreateReport(
            IReport report,
            VehicleWorkOrderCostReportInput input,
            Func<IVehicleWorkOrderCostTable> createVehicleWorkOrderCostTable
        )
        {
            report.AddReportHeader("Vehicle Work Order Cost Report");

            var vehicleWorkOrderCostItems = await GetVehicleWorkOrderCostItems(input);
            if (vehicleWorkOrderCostItems.Count == 0)
            {
                return false;
            }

            var vehicleWorkOrderCostTable = createVehicleWorkOrderCostTable();

            vehicleWorkOrderCostTable.AddColumnHeaders(
                "Office",
                "Vehicle",
                "Description",
                "Date",
                "WO #",
                "Service",
                "Note",
                "Labor Cost",
                "Parts Cost",
                "Tax",
                "Discount",
                "Total"
            );
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

            foreach (var item in vehicleWorkOrderCostItems)
            {
                vehicleWorkOrderCostTable.AddRow(
                    item.Office,
                    item.Vehicle,
                    item.Description,
                    item.CompletionDate?.ToString("d") ?? "",
                    item.Id.ToString(),
                    item.ServiceName,
                    item.Note,
                    item.LaborCost.ToString("C", currencyCulture),
                    item.PartsCost.ToString("C", currencyCulture),
                    item.Tax.ToString("N"),
                    item.Discount.ToString("N"),
                    item.TotalCost.ToString("C", currencyCulture)
                );
            }

            return true;
        }

        private async Task<List<VehicleWorkOrderCostItem>> GetVehicleWorkOrderCostItems(VehicleWorkOrderCostReportInput input)
        {
            return await (await _workOrderRepository.GetQueryAsync())
                .WhereIf(input.IssueDateBegin.HasValue, x => x.IssueDate >= input.IssueDateBegin.Value)
                .WhereIf(input.IssueDateEnd.HasValue, x => x.IssueDate <= input.IssueDateEnd.Value)
                .WhereIf(input.StartDateBegin.HasValue, x => x.StartDate >= input.StartDateBegin.Value)
                .WhereIf(input.StartDateEnd.HasValue, x => x.StartDate <= input.StartDateEnd.Value)
                .WhereIf(input.CompletionDateBegin.HasValue, x => x.CompletionDate >= input.CompletionDateBegin.Value)
                .WhereIf(input.CompletionDateEnd.HasValue, x => x.CompletionDate <= input.CompletionDateEnd.Value)
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(input.AssignedToId.HasValue, x => x.AssignedToId == input.AssignedToId)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(!input.OfficeIds.IsNullOrEmpty(), x => input.OfficeIds.Contains(x.Truck.Office.Id))
                .Select(x => new VehicleWorkOrderCostItem
                {
                    Id = x.Id,
                    Office = x.Truck.Office.Name,
                    Vehicle = x.Truck.TruckCode,
                    Description = x.Truck.Year.ToString() + " " + x.Truck.Make + " " + x.Truck.Model,
                    CompletionDate = x.CompletionDate,
                    ServiceName = x.VehicleServiceType.Name,
                    Note = x.Note,
                    LaborCost = x.TotalLaborCost,
                    PartsCost = x.TotalPartsCost,
                    Tax = x.Tax,
                    Discount = x.Discount,
                    TotalCost = x.TotalCost,
                })
                .ToListAsync();
        }
    }
}
