using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Drivers.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Drivers
{
    public class DriverAppService_GetDriversFromOrderLineSelectList_Tests : AppTestBase, IAsyncLifetime
    {
        private IDriverAppService _driverAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAppService = Resolve<IDriverAppService>();
        }

        [Fact]
        public async Task Test_GetDriversFromOrderLineSelectList_should_return_Drivers()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var truck2 = await CreateTruck();
            var driver = await CreateDriver();
            var driver2 = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, driver2.Id);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineId2 = order.OrderLines.Last().Id;
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLineId2, 1);

            // Act
            var result = await _driverAppService.GetDriversFromOrderLineSelectList(new GetDriversFromOrderLineSelectListInput
            {
                OrderLineId = orderLineId,
                MaxResultCount = 10,
            });

            // Assert
            result.Items.Count.ShouldBe(1);
            result.Items[0].Id.ShouldBe(driver.Id.ToString());
        }




        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
