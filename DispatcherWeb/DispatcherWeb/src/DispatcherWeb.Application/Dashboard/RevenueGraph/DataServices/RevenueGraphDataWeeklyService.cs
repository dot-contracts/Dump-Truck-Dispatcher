using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Utilities;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataServices
{
    [RemoteService(false)]
    public class RevenueGraphDataWeeklyService : IRevenueGraphDataWeeklyService
    {
        private readonly IRevenueGraphDataItemsQueryService _revenueGraphDataItemsQueryService;

        public RevenueGraphDataWeeklyService(
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService
        )
        {
            _revenueGraphDataItemsQueryService = revenueGraphDataItemsQueryService;
        }
        public async Task<GetRevenueGraphDataOutput> GetRevenueGraphData(PeriodInput input)
        {
            var firstDayOfTheWeek = input.PeriodBegin.StartOfWeek();

            var revenueByWeek = (from item in await _revenueGraphDataItemsQueryService.GetRevenueGraphDataItemsAsync(input)
                                 group item by new { Week = (item.DeliveryDate.Value - firstDayOfTheWeek).Days / 7 }
                        into g
                                 select new
                                 {
                                     g.Key.Week,
                                     MaterialRevenue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * olt.MaterialQuantity, 2)),
                                     FreightRevenue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.FreightPricePerUnit ?? 0) * olt.FreightQuantity, 2)),
                                     FuelSurcharge = g.Sum(olt => decimal.Round(olt.FuelSurcharge, 2)),
                                     InternalTrucksFuelSurcharge = g.Sum(olt => decimal.Round(olt.InternalTruckFuelSurcharge, 2)),
                                     LeaseHaulersFuelSurcharge = g.Sum(olt => decimal.Round(olt.LeaseHaulerFuelSurcharge, 2)),
                                 })
                    .OrderBy(x => x.Week)
                    .ToList()
                ;
            var allWeeksOfPeriod = Enumerable.Range(0, DateUtility.NumberOfWeeksBetweenDates(input.PeriodBegin, input.PeriodEnd) + 1);
            var revenueGraphData = (
                from w in allWeeksOfPeriod
                join rbw in revenueByWeek on w equals rbw.Week into revenueForEachWeekOfPeriod
                from r in revenueForEachWeekOfPeriod.DefaultIfEmpty()
                select new RevenueGraphData
                {
                    FreightRevenueValue = r?.FreightRevenue ?? 0,
                    MaterialRevenueValue = r?.MaterialRevenue ?? 0,
                    InternalTrucksFuelSurchargeValue = r?.InternalTrucksFuelSurcharge ?? 0,
                    LeaseHaulersFuelSurchargeValue = r?.LeaseHaulersFuelSurcharge ?? 0,
                    Period = FormatWeek(w),
                    PeriodStart = GetPeriodBegin(w),
                    PeriodEnd = GetPeriodEnd(w),
                }
            )
            .ToList();
            return new GetRevenueGraphDataOutput(revenueGraphData);

            // Local functions
            string FormatWeek(int week) => $"{GetPeriodBegin(week):d} - {GetPeriodEnd(week):d}";
            DateTime GetPeriodBegin(int week) => DateUtility.Max(input.PeriodBegin, input.PeriodBegin.AddDays(week * 7).StartOfWeek());
            DateTime GetPeriodEnd(int week) => DateUtility.Min(input.PeriodEnd, input.PeriodBegin.AddDays(week * 7).EndOfWeek());
        }
    }
}
