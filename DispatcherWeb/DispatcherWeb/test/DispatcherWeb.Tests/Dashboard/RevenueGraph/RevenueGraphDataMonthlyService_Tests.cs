using System.Threading.Tasks;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataMonthlyService_Tests : RevenueGraphDataService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Monthly_by_Tickets()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(30));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataMonthlyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var result = await revenueGraphDataService.GetRevenueGraphData(new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(30),
            });

            // Assert
            result.RevenueGraphData.Count.ShouldBe(1);
            result.RevenueGraphData[0].Period.ShouldBe(_startDate.ToString("MMMM yyyy"));
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10 * 31);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Monthly_by_Tickets_for_each_day_of_period()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(30));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataMonthlyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var periodInput = new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(31),
            };
            var result = await revenueGraphDataService.GetRevenueGraphData(periodInput);

            // Assert
            result.RevenueGraphData.Count.ShouldBe(2);
            result.RevenueGraphData[0].Period.ShouldBe(periodInput.PeriodBegin.ToString("MMMM yyyy"));
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10 * 31);
            result.RevenueGraphData[1].Period.ShouldBe(periodInput.PeriodEnd.ToString("MMMM yyyy"));
            result.RevenueGraphData[1].RevenueValue.ShouldBe(0);
        }

    }
}
