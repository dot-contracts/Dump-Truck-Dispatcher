using System;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Imports.Services;
using DispatcherWeb.Trucks;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Imports.Services
{
    public class UpdateTruckFromImportAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private IUpdateTruckFromImportAppService _updateTruckFromImportAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _updateTruckFromImportAppService = Resolve<IUpdateTruckFromImportAppService>();

        }

        [Fact]
        public async Task Test_UpdateMileageAndHours_should_update_MileageAndHours()
        {
            // Arrange
            var readingHours = 1234.56m;
            var readingMiles = 2345.67m;
            var truck = await CreateTruck();
            await UsingDbContextAsync(async context =>
            {
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = Clock.Now,
                    ReadingType = ReadingType.Hours,
                    Reading = readingHours,
                });
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = Clock.Now,
                    ReadingType = ReadingType.Miles,
                    Reading = readingMiles,
                });
            });

            // Act
            await _updateTruckFromImportAppService.UpdateMileageAndHoursAsync(1, 1);

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentHours.ShouldBe(readingHours);
            updatedTruck.CurrentMileage.ShouldBe((int)readingMiles);
        }

        [Fact]
        public async Task Test_UpdateMileageAndHours_should_update_MileageAndHours_with_latest_values()
        {
            // Arrange
            decimal readingHours = 100;
            decimal readingMiles = 200;
            var date1 = new DateTime(2019, 5, 1);
            var date2 = new DateTime(2019, 5, 2);
            var truck = await CreateTruck();
            await UsingDbContextAsync(async context =>
            {
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = date1,
                    ReadingType = ReadingType.Hours,
                    Reading = 10,
                });
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = date2,
                    ReadingType = ReadingType.Hours,
                    Reading = readingHours,
                });
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = date1,
                    ReadingType = ReadingType.Miles,
                    Reading = 20,
                });
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = date2,
                    ReadingType = ReadingType.Miles,
                    Reading = readingMiles,
                });
            });

            // Act
            await _updateTruckFromImportAppService.UpdateMileageAndHoursAsync(1, 1);

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentHours.ShouldBe(readingHours);
            updatedTruck.CurrentMileage.ShouldBe((int)readingMiles);
        }

        [Fact]
        public async Task Test_UpdateMileageAndHours_should_not_update_MileageAndHours_with_less_values()
        {
            // Arrange
            decimal readingHours = 1;
            decimal readingMiles = 1;
            var truck = await CreateTruck();
            await UpdateEntity(truck, t =>
            {
                t.CurrentHours = 10;
                t.CurrentMileage = 10;
            });
            await UsingDbContextAsync(async context =>
            {
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = Clock.Now,
                    ReadingType = ReadingType.Hours,
                    Reading = readingHours,
                });
                await context.VehicleUsages.AddAsync(new VehicleUsage
                {
                    TenantId = 1,
                    TruckId = truck.Id,
                    ReadingDateTime = Clock.Now,
                    ReadingType = ReadingType.Miles,
                    Reading = readingMiles,
                });
            });

            // Act
            await _updateTruckFromImportAppService.UpdateMileageAndHoursAsync(1, 1);

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentHours.ShouldBe(truck.CurrentHours);
            updatedTruck.CurrentMileage.ShouldBe(truck.CurrentMileage);
        }

        [Fact]
        public async Task Test_UpdateMileageAndHours_should_not_update_MileageAndHours_when_there_are_no_values()
        {
            // Arrange
            var truck = await CreateTruck();

            // Act
            await _updateTruckFromImportAppService.UpdateMileageAndHoursAsync(1, 1);

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentHours.ShouldBe(truck.CurrentHours);
            updatedTruck.CurrentMileage.ShouldBe(truck.CurrentMileage);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
