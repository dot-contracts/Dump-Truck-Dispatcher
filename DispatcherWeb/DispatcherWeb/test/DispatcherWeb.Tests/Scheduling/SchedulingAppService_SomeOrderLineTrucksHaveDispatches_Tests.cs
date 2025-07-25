using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_SomeOrderLineTrucksHaveDispatches_Tests : SchedulingAppService_Tests_Base
    {
        [Theory]
        [InlineData(null, null, false, false)]
        [InlineData(DispatchStatus.Completed, null, false, false)]
        [InlineData(DispatchStatus.Created, null, true, false)]
        [InlineData(null, DispatchStatus.Created, true, false)]
        [InlineData(DispatchStatus.Created, DispatchStatus.Created, true, false)]
        [InlineData(DispatchStatus.Created, DispatchStatus.Acknowledged, true, true)]
        [InlineData(DispatchStatus.Acknowledged, DispatchStatus.Loaded, false, true)]
        [InlineData(DispatchStatus.Sent, DispatchStatus.Loaded, true, true)]
        public async Task Test_SomeOrderLineTrucksHaveDispatches_should_return_HasDispatchesResult(
            DispatchStatus? dispatchStatus1,
            DispatchStatus? dispatchStatus2,
            bool unacknowledged,
            bool acknowledged
        )
        {
            // Arrange
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var driver = await CreateDriver();
            var truck1 = await CreateTruck();
            var orderLineTruck1 = await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine.Id, 1);
            if (dispatchStatus1.HasValue)
            {
                await CreateDispatch(truck1.Id, driver.Id, orderLine.Id, dispatchStatus1.Value);
            }
            var truck2 = await CreateTruck();
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine.Id, 1);
            if (dispatchStatus2.HasValue)
            {
                await CreateDispatch(truck2.Id, driver.Id, orderLine.Id, dispatchStatus2.Value);
            }

            // Act
            var result = await _schedulingAppService.SomeOrderLineTrucksHaveDispatches(new SomeOrderLineTrucksHaveDispatchesInput
            {
                OrderLineId = orderLine.Id,
                TruckIds = new[] { truck1.Id, truck2.Id },
            });

            // Assert
            result.ShouldNotBeNull();
            result.Unacknowledged.ShouldBe(unacknowledged);
            result.AcknowledgedOrLoaded.ShouldBe(acknowledged);
        }
    }
}
