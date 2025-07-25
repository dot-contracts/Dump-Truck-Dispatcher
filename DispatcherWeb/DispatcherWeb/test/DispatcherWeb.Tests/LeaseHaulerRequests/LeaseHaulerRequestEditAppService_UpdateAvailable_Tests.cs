using System;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestEditAppService_UpdateAvailable_Tests : LeaseHaulerRequestEditAppService_Tests_Base
    {
        [Fact]
        public async Task Test_UpdateAvailable_should_update_Available_value()
        {
            // Arrange
            var date = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerRequest = await CreateLeaseHaulerRequest(date, Shift.Shift1, _officeId, leaseHauler.Id);
            const int newAvailableValue = 3;

            // Act
            await _leaseHaulerRequestEditAppService.UpdateAvailable(new IdValueInput<int?>() { Id = leaseHaulerRequest.Id, Value = newAvailableValue });

            // Assert
            var updatedLeaseHaulerRequest = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.FindAsync(leaseHaulerRequest.Id));
            updatedLeaseHaulerRequest.Available.ShouldBe(newAvailableValue);
            updatedLeaseHaulerRequest.Approved.ShouldBe(leaseHaulerRequest.Approved);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(null)]
        public async Task Test_UpdateAvailable_should_throw_ArgumentException_when_Available_is_less_than_Approved(int? available)
        {
            // Arrange
            var date = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerRequest = await CreateLeaseHaulerRequest(date, Shift.Shift1, _officeId, leaseHauler.Id, 10, approved: 5);

            // Act, Assert
            await _leaseHaulerRequestEditAppService.UpdateAvailable(new IdValueInput<int?>() { Id = leaseHaulerRequest.Id, Value = available }).ShouldThrowAsync(typeof(ArgumentException));

        }
    }
}
