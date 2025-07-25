using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Timing;
using DispatcherWeb.Features;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_GetScheduleTrucks_Tests : AppTestBase, IAsyncLifetime
    {
        private ISchedulingAppService _schedulingAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _schedulingAppService = Resolve<ISchedulingAppService>();
        }


        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetScheduleTrucks_should_return_Truck_with_HasNoDriver_is_true_for_the_past_date_without_DriverAssignment()
        {
            // Assign
            var truckEntity = await CreateTruck();
            await SetDefaultDriverForTruck(truckEntity.Id);
            var pastDate = Clock.Now.Date.AddDays(-1);

            // Act
            var trucksResult = await _schedulingAppService.GetScheduleTrucks(new GetScheduleTrucksInput
            {
                Date = pastDate,
                OfficeId = _officeId,
            });

            // Assert
            trucksResult.Items.Count.ShouldBe(1);
            var scheduleTruckDto = trucksResult.Items[0];
            scheduleTruckDto.HasDefaultDriver.ShouldBeFalse();
            scheduleTruckDto.HasNoDriver.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_GetScheduleTrucks_should_return_Truck_with_HasNoDriver_is_false_for_the_current_date_without_DriverAssignment()
        {
            // Assign
            var truckEntity = await CreateTruck();
            await SetDefaultDriverForTruck(truckEntity.Id);
            var today = Clock.Now.Date;

            // Act
            var trucksResult = await _schedulingAppService.GetScheduleTrucks(new GetScheduleTrucksInput
            {
                Date = today,
                OfficeId = _officeId,
            });

            // Assert
            trucksResult.Items.Count.ShouldBe(1);
            var scheduleTruckDto = trucksResult.Items[0];
            scheduleTruckDto.HasDefaultDriver.ShouldBeTrue();
            scheduleTruckDto.HasNoDriver.ShouldBeFalse();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetScheduleTrucks_should_return_LeaseHaulerTruck_with_Driver()
        {
            // Assign
            var featureChecker = Substitute.For<IFeatureChecker>();
            featureChecker.GetValueAsync(AppFeatures.AllowLeaseHaulersFeature).Returns("true");
            ((ApplicationService)_schedulingAppService).FeatureChecker = featureChecker;
            var today = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerDriver = await CreateLeaseHaulerDriver(leaseHauler.Id);
            var leaseHaulerTruck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            await UpdateEntity(leaseHaulerTruck, t => t.DefaultDriverId = leaseHaulerDriver.Id);
            await CreateAvailableLeaseHaulerTruck(leaseHauler.Id, leaseHaulerTruck.Id, leaseHaulerDriver.Id, today, Shift.Shift1, _officeId);

            // Act
            var trucksResult = await _schedulingAppService.GetScheduleTrucks(new GetScheduleTrucksInput
            {
                Date = today,
                Shift = Shift.Shift1,
                OfficeId = _officeId,
            });

            // Assert
            trucksResult.Items.Count.ShouldBe(1);
            var scheduleTruckDto = trucksResult.Items[0];
            scheduleTruckDto.HasDefaultDriver.ShouldBeTrue();
            scheduleTruckDto.DriverName.ShouldBe($"{leaseHaulerDriver.FirstName} {leaseHaulerDriver.LastName}");
            scheduleTruckDto.HasNoDriver.ShouldBeFalse();
        }


        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
