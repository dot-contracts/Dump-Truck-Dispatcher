using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataServices
{
    [RemoteService(false)]
    public class RevenueGraphTotalService : IRevenueGraphDataTotalService
    {
        private readonly IRevenueGraphDataItemsQueryService _revenueGraphDataItemsQueryService;

        public RevenueGraphTotalService(
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService
        )
        {
            _revenueGraphDataItemsQueryService = revenueGraphDataItemsQueryService;
        }

        public async Task<GetRevenueGraphDataOutput> GetRevenueGraphData(PeriodInput input)
        {
            var revenueByDay =
                   (from item in await _revenueGraphDataItemsQueryService.GetRevenueGraphDataItemsAsync(input)
                    group item by item.DeliveryDate.Value
                   into g
                    select new
                    {
                        BeginOfPeriod = g.Key,
                        MaterialRevenue = g.Sum(olt => olt.IsMaterialPriceOverridden ? decimal.Round(olt.MaterialPriceOriginal, 2) : decimal.Round((olt.MaterialPricePerUnit ?? 0) * olt.MaterialQuantity, 2)),
                        FreightRevenue = g.Sum(olt => olt.IsFreightPriceOverridden ? decimal.Round(olt.FreightPriceOriginal, 2) : decimal.Round((olt.FreightPricePerUnit ?? 0) * olt.FreightQuantity, 2)),
                        InternalTrucksFuelSurcharge = g.Sum(olt => decimal.Round(olt.InternalTruckFuelSurcharge, 2)),
                        LeaseHaulersFuelSurcharge = g.Sum(olt => decimal.Round(olt.LeaseHaulerFuelSurcharge, 2)),
                    })
                   .ToList()
               ;
            var allDaysOfPefiod =
                Enumerable.Range(0, 1 + input.PeriodEnd.Subtract(input.PeriodBegin).Days)
                .Select(offset => input.PeriodBegin.AddDays(offset))
                ;
            var revenueGraphData = (
                    from d in allDaysOfPefiod
                    join rbd in revenueByDay on d equals rbd.BeginOfPeriod into revenueForEachDayOfPeriod
                    from r in revenueForEachDayOfPeriod.DefaultIfEmpty()
                    select new RevenueGraphData
                    {
                        FreightRevenueValue = r?.FreightRevenue ?? 0,
                        MaterialRevenueValue = r?.MaterialRevenue ?? 0,
                        InternalTrucksFuelSurchargeValue = r?.InternalTrucksFuelSurcharge ?? 0,
                        LeaseHaulersFuelSurchargeValue = r?.LeaseHaulersFuelSurcharge ?? 0,
                        Period = d.ToString("yyyy-MM-dd"),
                        PeriodStart = d,
                        PeriodEnd = d,
                    }
                ).ToList();
            return new GetRevenueGraphDataOutput(revenueGraphData);
        }

    }
}
