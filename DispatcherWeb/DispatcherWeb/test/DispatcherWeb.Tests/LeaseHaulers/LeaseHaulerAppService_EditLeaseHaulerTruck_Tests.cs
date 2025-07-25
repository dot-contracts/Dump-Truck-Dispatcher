using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Tests.LeaseHaulerRequests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulers
{
    public class LeaseHaulerAppService_EditLeaseHaulerTruck_Tests : LeaseHaulerRequestAppService_Tests_Base, IAsyncLifetime
    {
        protected ILeaseHaulerAppService _leaseHaulerAppService;

        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _leaseHaulerAppService = Resolve<ILeaseHaulerAppService>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHaulerTruck_should_create_Truck()
        {
            // Arrange
            var dumpTrucks = await CreateVehicleCategory();
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            const string truckCode = "LH002";
            var model = new LeaseHaulerTruckEditDto
            {
                Id = null,
                LeaseHaulerId = leaseHauler.Id,
                TruckCode = truckCode,
                VehicleCategoryId = dumpTrucks.Id,
                IsActive = true,
            };

            // Act
            await _leaseHaulerAppService.EditLeaseHaulerTruck(model);

            // Assert
            var leaseHaulerTrucks = await UsingDbContextAsync(async context => await context.LeaseHaulerTrucks.Include(lht => lht.Truck).ToListAsync());
            leaseHaulerTrucks.Count.ShouldBe(2);
            var createdTruck = leaseHaulerTrucks.First(lht => lht.Id != truck.Id).Truck;
            createdTruck.LeaseHaulerTruck.LeaseHaulerId.ShouldBe(leaseHauler.Id);
            createdTruck.TruckCode.ShouldBe(model.TruckCode);
            createdTruck.VehicleCategoryId.ShouldBe(model.VehicleCategoryId);
            createdTruck.IsActive.ShouldBe(model.IsActive);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHaulerTruck_should_throw_UserFriendlyException_when_creates_Truck_with_same_TruckCode_and_LeaseHauler_exist()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            var model = new LeaseHaulerTruckEditDto
            {
                Id = null,
                LeaseHaulerId = leaseHauler.Id,
                TruckCode = truck.TruckCode,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = true,
            };

            // Act, Assert
            await _leaseHaulerAppService.EditLeaseHaulerTruck(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHaulerTruck_should_not_throw_UserFriendlyException_when_creates_Truck_with_same_TruckCode_and_another_LeaseHauler()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var leaseHauler2 = await CreateLeaseHauler("LeaseHauler2");
            var truck = await CreateLeaseHaulerTruck(leaseHauler2.Id);
            var model = new LeaseHaulerTruckEditDto
            {
                Id = null,
                LeaseHaulerId = leaseHauler.Id,
                TruckCode = truck.TruckCode,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = true,
            };

            // Act
            await _leaseHaulerAppService.EditLeaseHaulerTruck(model);

            // Assert
            var leaseHaulerTrucks = await UsingDbContextAsync(async context => await context.LeaseHaulerTrucks.Include(lht => lht.Truck).ToListAsync());
            leaseHaulerTrucks.Count.ShouldBe(2);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHaulerTruck_should_throw_UserFriendlyException_when_edits_Truck_with_same_TruckCode_and_LeaseHauler_exist()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            var truck2 = await CreateLeaseHaulerTruck(leaseHauler.Id, null, "LH002");
            var model = new LeaseHaulerTruckEditDto
            {
                Id = truck.Id,
                LeaseHaulerId = leaseHauler.Id,
                TruckCode = truck2.TruckCode,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = true,
            };

            // Act, Assert
            await _leaseHaulerAppService.EditLeaseHaulerTruck(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }
    }
}
