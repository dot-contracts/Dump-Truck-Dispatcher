using System;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class DriverAssignmentAppService_AddDefaultStartTime_Tests : AppTestBase, IAsyncLifetime
    {
        private int _officeId;
        private IDriverAssignmentAppService _driverAssignmentAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAssignmentAppService = Resolve<IDriverAssignmentAppService>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_AddDefaultStartTime_should_update_StartTime_of_existing_DriverAssignment_from_Order_StartTime()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment, da => da.StartTime = date.AddHours(12).AddMinutes(00));

            // Act
            await _driverAssignmentAppService.AddDefaultStartTime(new AddDefaultStartTimeInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            var updatedDriverAssignment = await UsingDbContextAsync(async context => await context.DriverAssignments.FindAsync(driverAssignment.Id));
            updatedDriverAssignment.StartTime.ShouldNotBeNull();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_AddDefaultStartTime_should_not_update_StartTime_of_existing_DriverAssignment_from_Order_on_another_Date()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var order = await CreateOrderWithOrderLines(date.AddDays(1), Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);

            // Act
            await _driverAssignmentAppService.AddDefaultStartTime(new AddDefaultStartTimeInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            var updatedDriverAssignment = await UsingDbContextAsync(async context => await context.DriverAssignments.FindAsync(driverAssignment.Id));
            updatedDriverAssignment.StartTime.ShouldBeNull();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_AddDefaultStartTime_should_not_update_StartTime_of_existing_DriverAssignment_from_not_associated_Order()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);

            // Act
            await _driverAssignmentAppService.AddDefaultStartTime(new AddDefaultStartTimeInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            var updatedDriverAssignment = await UsingDbContextAsync(async context => await context.DriverAssignments.FindAsync(driverAssignment.Id));
            updatedDriverAssignment.StartTime.ShouldBeNull();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
