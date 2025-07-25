using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Drivers;
using DispatcherWeb.Drivers.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Drivers
{
    public class DriverAppService_EditDriver_Tests : AppTestBase, IAsyncLifetime
    {
        private IDriverAppService _driverAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAppService = Resolve<IDriverAppService>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditDriver_should_set_DriverAssignment_null_and_remove_OrderLineTruck_for_current_and_future_dates_and_remove_DefaultDriver_when_Driver_is_inactive()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);

            // Act
            await _driverAppService.EditDriver(new DriverEditDto
            {
                Id = driver.Id,
                IsInactive = true,
                OfficeId = truck.OfficeId,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.DefaultDriverId.ShouldBeNull();
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditDriver_should_set_DriverAssignment_and_remove_OrderLineTruck_for_current_and_future_dates_when_Driver_is_inactive()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);

            // Act
            await _driverAppService.EditDriver(new DriverEditDto
            {
                Id = driver.Id,
                IsInactive = true,
                OfficeId = truck.OfficeId,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.DefaultDriverId.ShouldBeNull();
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_EditDriver_should_throw_UserFriendlyException_when_Driver_is_inactive_and_there_are_Dispatches()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Acknowledged);

            // Act, Assert
            await _driverAppService.EditDriver(new DriverEditDto
            {
                Id = driver.Id,
                IsInactive = true,
                OfficeId = truck.OfficeId,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
