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
    public class RevenueGraphDataMonthlyService : IRevenueGraphDataMonthlyService
    {
        private readonly IRevenueGraphDataItemsQueryService _revenueGraphDataItemsQueryService;

        public RevenueGraphDataMonthlyService(
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService
        )
        {
            _revenueGraphDataItemsQueryService = revenueGraphDataItemsQueryService;
        }

        public async Task<GetRevenueGraphDataOutput> GetRevenueGraphData(PeriodInput input)
        {
            var revenueByMonth =
                    (from item in await _revenueGraphDataItemsQueryService.GetRevenueGraphDataItemsAsync(input)
                     group item by new { item.DeliveryDate.Value.Month, item.DeliveryDate.Value.Year }
                        into g
                     select new
                     {
                         BeginOfPeriod = new DateTime(g.Key.Year, g.Key.Month, 1),
                         MaterialRevenue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * olt.MaterialQuantity, 2)),
                         FreightRevenue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.FreightPricePerUnit ?? 0) * olt.FreightQuantity, 2)),
                         FuelSurcharge = g.Sum(olt => decimal.Round(olt.FuelSurcharge, 2)),
                         InternalTrucksFuelSurcharge = g.Sum(olt => decimal.Round(olt.InternalTruckFuelSurcharge, 2)),
                         LeaseHaulersFuelSurcharge = g.Sum(olt => decimal.Round(olt.LeaseHaulerFuelSurcharge, 2)),
                     })
                    .ToList()
                ;
            var allMonthsOfPeriod = Enumerable.Range(0, DateUtility.NumberOfMonthsBetweenDates(input.PeriodBegin, input.PeriodEnd) + 1);
            var revenueGraphData = (
                    from m in allMonthsOfPeriod
                    join rbm in revenueByMonth on m equals GetDateOffsetInMonths(rbm.BeginOfPeriod) into revenueForEachMonthOfPeriod
                    from r in revenueForEachMonthOfPeriod.DefaultIfEmpty()
                    select new RevenueGraphData
                    {
                        FreightRevenueValue = r?.FreightRevenue ?? 0,
                        MaterialRevenueValue = r?.MaterialRevenue ?? 0,
                        InternalTrucksFuelSurchargeValue = r?.InternalTrucksFuelSurcharge ?? 0,
                        LeaseHaulersFuelSurchargeValue = r?.LeaseHaulersFuelSurcharge ?? 0,
                        Period = input.PeriodBegin.AddMonths(m).ToString("MMMM yyyy"),
                        PeriodStart = GetPeriodBegin(m),
                        PeriodEnd = GetPeriodEnd(m),
                    }
                )
                .ToList();

            return new GetRevenueGraphDataOutput(revenueGraphData);

            // Local functions
            int GetDateOffsetInMonths(DateTime date) =>
                (date.Year - input.PeriodBegin.Year) * 12 +
                    date.Year == input.PeriodBegin.Year ? date.Month - input.PeriodBegin.Month : 12 - input.PeriodBegin.Month + date.Month;
            DateTime GetPeriodBegin(int m) => DateUtility.Max(input.PeriodBegin, input.PeriodBegin.AddMonths(m).StartOfMonth());
            DateTime GetPeriodEnd(int m) => DateUtility.Min(input.PeriodEnd, input.PeriodBegin.AddMonths(m).EndOfMonth());
        }

    }
}
