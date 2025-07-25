using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulers
{
    public class LeaseHaulerAppService_DeleteLeaseHauler_Tests : LeaseHaulerAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DeleteLeaseHauler_should_delete_LeaseHauler_when_there_is_no_Truck_or_Driver()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();

            // Act
            await _leaseHaulerAppService.DeleteLeaseHauler(new EntityDto(leaseHauler.Id));

            // Assert
            var leaseHaulers = await UsingDbContextAsync(async context => await context.LeaseHaulers.ToListAsync());
            leaseHaulers.Count.ShouldBe(1);
            leaseHaulers.First().IsDeleted.ShouldBeTrue();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DeleteLeaseHauler_should_throw_UserFriendlyException_when_LeaseHauler_has_Truck()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);

            // Act, Assert
            await _leaseHaulerAppService.DeleteLeaseHauler(new EntityDto(leaseHauler.Id)).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DeleteLeaseHauler_should_throw_UserFriendlyException_when_LeaseHauler_has_Driver()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerDriver(leaseHauler.Id);

            // Act, Assert
            await _leaseHaulerAppService.DeleteLeaseHauler(new EntityDto(leaseHauler.Id)).ShouldThrowAsync(typeof(UserFriendlyException));
        }
    }
}
