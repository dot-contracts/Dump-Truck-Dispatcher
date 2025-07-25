using System.Threading.Tasks;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataDailyService_Tests : RevenueGraphDataService_Tests_Base
    {

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Daily_by_Tickets()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(30));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataDailyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var result = await revenueGraphDataService.GetRevenueGraphData(new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(30),
            });

            // Assert
            result.RevenueGraphData.Count.ShouldBe(31);
            result.RevenueGraphData[0].Period.ShouldBe(_startDate.ToString("yyyy-MM-dd"));
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Daily_by_Tickets_for_each_day_of_period()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(0));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataDailyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var periodInput = new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(30),
            };
            var result = await revenueGraphDataService.GetRevenueGraphData(periodInput);

            // Assert
            int numberOfDays = (int)(periodInput.PeriodEnd - periodInput.PeriodBegin).TotalDays + 1;
            result.RevenueGraphData.Count.ShouldBe(numberOfDays);
            result.RevenueGraphData[0].Period.ShouldBe(_startDate.ToString("yyyy-MM-dd"));
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10);

            for (int i = 1; i < numberOfDays; i++)
            {
                result.RevenueGraphData[i].Period.ShouldBe(_startDate.AddDays(i).ToString("yyyy-MM-dd"));
                result.RevenueGraphData[i].RevenueValue.ShouldBe(0);
            }
        }
    }
}
