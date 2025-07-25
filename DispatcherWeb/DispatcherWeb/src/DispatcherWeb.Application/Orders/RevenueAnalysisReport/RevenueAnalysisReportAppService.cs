using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Orders.RevenueAnalysisReport.Dto;

namespace DispatcherWeb.Orders.RevenueAnalysisReport
{
    public class RevenueAnalysisReportAppService : DispatcherWebAppServiceBase, IRevenueAnalysisReportAppService
    {
        private readonly IRevenueGraphByTicketsDataItemsQueryService _revenueGraphDataItemsQueryService;

        public RevenueAnalysisReportAppService(
            IRevenueGraphByTicketsDataItemsQueryService revenueGraphDataItemsQueryService
            )
        {
            _revenueGraphDataItemsQueryService = revenueGraphDataItemsQueryService;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_RevenueAnalysis)]
        public async Task<RevenueAnalysisReportOutput> GetRevenueAnalysis(RevenueAnalysisReportInput input)
        {
            var queryInput = new PeriodInput
            {
                PeriodBegin = input.DeliveryDateBegin,
                PeriodEnd = input.DeliveryDateEnd,
                TicketType = input.HasLeaseHaulerId.HasValue
                    ? (input.HasLeaseHaulerId.Value ? Dashboard.Dto.TicketType.LeaseHaulers : Dashboard.Dto.TicketType.InternalTrucks)
                    : Dashboard.Dto.TicketType.Both,
                OfficeId = input.OfficeId,
            };

            var data = await _revenueGraphDataItemsQueryService.GetRevenueGraphDataItemsAsync(queryInput);
            IEnumerable<IGrouping<string, RevenueGraphDataItem>> grouping;

            switch (input.AnalyzeBy)
            {
                case AnalyzeRevenueBy.Driver:
                    grouping = data.GroupBy(x => x.DriverName);
                    break;

                case AnalyzeRevenueBy.Truck:
                    grouping = data.GroupBy(x => x.TruckCode);
                    break;

                case AnalyzeRevenueBy.Customer:
                    grouping = data.GroupBy(x => x.CustomerName);
                    break;

                case AnalyzeRevenueBy.Date:
                    grouping = data.GroupBy(x => x.DeliveryDate?.ToShortDateString());
                    break;

                default:
                    throw new ArgumentException(nameof(input.AnalyzeBy));
            }

            return new RevenueAnalysisReportOutput(grouping
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new RevenueAnalysisReportDataItem
                {
                    AnalysisBy = g.Key,
                    MaterialRevenueValue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * olt.MaterialQuantity, 2)),
                    FreightRevenueValue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.FreightPricePerUnit ?? 0) * olt.FreightQuantity, 2)),
                    InternalTrucksFuelSurchargeValue = g.Sum(olt => decimal.Round(olt.InternalTruckFuelSurcharge, 2)),
                    LeaseHaulersFuelSurchargeValue = g.Sum(olt => decimal.Round(olt.LeaseHaulerFuelSurcharge, 2)),
                })
                .OrderByDescending(x => x.RevenueValue)
                .Take(100)
                .ToList());
        }

    }
}
