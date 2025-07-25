using System.Threading.Tasks;
using DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices;
using DispatcherWeb.Dashboard.RevenueGraph.DataServices;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Utilities;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dashboard.RevenueGraph
{
    public class RevenueGraphDataWeeklyService_Tests : RevenueGraphDataService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Monthly_by_Tickets()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(30));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataWeeklyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var periodInput = new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(30),
            };
            var result = await revenueGraphDataService.GetRevenueGraphData(periodInput);

            // Assert
            result.RevenueGraphData.Count.ShouldBe(6);
            var date = _startDate;
            result.RevenueGraphData[0].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10 * 2);
            date = date.AddDays(7).StartOfWeek();
            result.RevenueGraphData[1].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[1].RevenueValue.ShouldBe((2 + 3) * 10 * 7);
            date = date.AddDays(7).StartOfWeek();
            result.RevenueGraphData[2].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[2].RevenueValue.ShouldBe((2 + 3) * 10 * 7);
            date = date.AddDays(7).StartOfWeek();
            result.RevenueGraphData[3].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[3].RevenueValue.ShouldBe((2 + 3) * 10 * 7);
            date = date.AddDays(7).StartOfWeek();
            result.RevenueGraphData[4].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[4].RevenueValue.ShouldBe((2 + 3) * 10 * 7);
            date = date.AddDays(7).StartOfWeek();
            result.RevenueGraphData[5].Period.ShouldBe($"{date:d} - {periodInput.PeriodEnd:d}");
            result.RevenueGraphData[5].RevenueValue.ShouldBe((2 + 3) * 10 * 1);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRevenueGraphData_should_return_RevenueGraphData_by_Monthly_by_Tickets_for_each_day_of_period()
        {
            // Arrange
            await CreateOrdersAndOrderLinesAndTickets(_startDate, _startDate.AddDays(0));
            IRevenueGraphDataItemsQueryService revenueGraphDataItemsQueryService = Resolve<IRevenueGraphByTicketsDataItemsQueryService>();
            var revenueGraphDataService = Resolve<IRevenueGraphDataWeeklyService>(new { revenueGraphDataItemsQueryService });

            // Act
            var periodInput = new PeriodInput
            {
                PeriodBegin = _startDate,
                PeriodEnd = _startDate.AddDays(30),
            };
            var result = await revenueGraphDataService.GetRevenueGraphData(periodInput);

            // Assert
            int numberOfWeeks = 6;
            result.RevenueGraphData.Count.ShouldBe(numberOfWeeks);
            var date = _startDate;
            result.RevenueGraphData[0].Period.ShouldBe($"{date:d} - {date.EndOfWeek():d}");
            result.RevenueGraphData[0].RevenueValue.ShouldBe((2 + 3) * 10 * 1);
            for (int i = 1; i < numberOfWeeks; i++)
            {
                date = date.AddDays(7).StartOfWeek();
                result.RevenueGraphData[i].Period.ShouldBe($"{date:d} - {DateUtility.Min(date.EndOfWeek(), periodInput.PeriodEnd):d}");
                result.RevenueGraphData[i].RevenueValue.ShouldBe(0);
            }
        }


    }
}
