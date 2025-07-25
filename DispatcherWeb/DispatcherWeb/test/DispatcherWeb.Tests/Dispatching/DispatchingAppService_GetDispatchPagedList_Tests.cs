using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dispatching.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_GetDispatchPagedList_Tests : DispatchingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_GetDispatchPagedList_should_return_list_of_dispatches_for_each_Load()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);
            await CreateLoad(dispatch.Id, today, today);
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Loads.Count.ShouldBe(2);

            // Act 
            var result = await _dispatchingAppService.GetDispatchPagedList(new GetDispatchPagedListInput());

            // Assert
            result.TotalCount.ShouldBe(2);
            result.Items.Count.ShouldBe(2);
        }

    }
}
