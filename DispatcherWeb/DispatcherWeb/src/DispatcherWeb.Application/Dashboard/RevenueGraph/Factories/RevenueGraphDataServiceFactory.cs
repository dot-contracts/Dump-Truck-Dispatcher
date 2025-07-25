using System;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.Factories
{
    public static class RevenueGraphDataServiceFactory
    {
        public static IRevenueGraphDataService CreateRevenueGraphDataService(
            RevenueGraphDatePeriod revenueGraphDatePeriod,
            IRevenueGraphByTicketsDataItemsQueryService revenueGraphDataItemsQueryService
        )
        {
            switch (revenueGraphDatePeriod)
            {
                case RevenueGraphDatePeriod.Daily:
                    return new RevenueGraphDataDailyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Weekly:
                    return new RevenueGraphDataWeeklyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Monthly:
                    return new RevenueGraphDataMonthlyService(revenueGraphDataItemsQueryService);
                case RevenueGraphDatePeriod.Total:
                    return new RevenueGraphTotalService(revenueGraphDataItemsQueryService);
                default:
                    throw new ArgumentOutOfRangeException($"The unknown RevenueGraphDatePeriod: {revenueGraphDatePeriod}!");
            }
        }

    }
}
