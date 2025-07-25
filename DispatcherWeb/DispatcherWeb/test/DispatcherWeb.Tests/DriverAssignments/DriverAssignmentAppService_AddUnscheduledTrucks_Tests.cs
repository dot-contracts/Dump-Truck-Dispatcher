using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class DriverAssignmentAppService_AddUnscheduledTrucks_Tests : AppTestBase, IAsyncLifetime
    {
        private int _officeId;
        private IDriverAssignmentAppService _driverAssignmentAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAssignmentAppService = Resolve<IDriverAssignmentAppService>();
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_create_DriverAssignment_with_DefaultDriver_when_shift_is_Shift1()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var driver = await SetDefaultDriverForTruck(truck.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(1);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBe(Shift.Shift1);
            driverAssignments[0].DriverId.ShouldBe(driver.Id);
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_create_DriverAssignment_with_DefaultDriver_when_shift_is_null()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var driver = await SetDefaultDriverForTruck(truck.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = null,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(1);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBeNull();
            driverAssignments[0].DriverId.ShouldBe(driver.Id);
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_create_DriverAssignment_with_null_when_shift_is_null_and_there_is_no_DefaultDriver()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = null,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(1);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBeNull();
            driverAssignments[0].DriverId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_create_DriverAssignment_with_null_when_shift_is_2()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var driver = await SetDefaultDriverForTruck(truck.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = Shift.Shift2,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(1);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBe(Shift.Shift2);
            driverAssignments[0].DriverId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_create_DriverAssignment_with_null_when_shift_is_3()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var driver = await SetDefaultDriverForTruck(truck.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = Shift.Shift3,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(1);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Shift.ShouldBe(Shift.Shift3);
            driverAssignments[0].DriverId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_not_create_DriverAssignment_when_truck_has_DriverAssignment_for_Date_and_Shift_is_null()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, driver.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = null,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(0);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Date.ShouldBe(date);
            driverAssignments[0].Id.ShouldBe(driverAssignment.Id);
            driverAssignments[0].Shift.ShouldBeNull();
            driverAssignments[0].DriverId.ShouldBe(driver.Id);
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_not_create_DriverAssignment_for_Trailers_and_LeaseHaulers()
        {
            // Arrange
            var date = Clock.Now.Date;
            var dumpTrucks = await CreateVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var truck = await CreateTruck("101", trailers, _officeId);
            var truck2 = await CreateTruck("102", dumpTrucks, _officeId, alwaysShowOnSchedule: true);
            var truck3 = await CreateTruck("102", tractors, _officeId, alwaysShowOnSchedule: true, canPullTrailer: true);
            await SetDefaultDriverForTruck(truck.Id);
            await SetDefaultDriverForTruck(truck2.Id);
            await SetDefaultDriverForTruck(truck3.Id);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(0);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_AddUnscheduledTrucks_should_not_create_DriverAssignment_for_Inactive_and_OutOfService_trucks()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck("101", null, _officeId);
            var truck2 = await CreateTruck("102", null, _officeId);
            await SetDefaultDriverForTruck(truck.Id);
            await SetDefaultDriverForTruck(truck2.Id);
            truck = await UpdateEntity(truck, t => t.IsActive = false);
            truck2 = await UpdateEntity(truck2, t => t.IsOutOfService = true);

            // Act
            var result = await _driverAssignmentAppService.AddUnscheduledTrucks(new AddUnscheduledTrucksInput
            {
                Date = date,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            result.ShouldBe(0);
            var driverAssignments = await GetDriverAssignmnets();
            driverAssignments.Count.ShouldBe(0);
        }


        private async Task<IList<DriverAssignment>> GetDriverAssignmnets() =>
            await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
