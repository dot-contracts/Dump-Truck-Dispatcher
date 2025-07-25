using System;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestEditAppService_UpdateApproved_Tests : LeaseHaulerRequestEditAppService_Tests_Base
    {
        [Fact]
        public async Task Test_UpdateAvailable_should_update_Available_value()
        {
            // Arrange
            var date = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerRequest = await CreateLeaseHaulerRequest(date, Shift.Shift1, _officeId, leaseHauler.Id, 10);
            const int newApprovedValue = 3;

            // Act
            await _leaseHaulerRequestEditAppService.UpdateApproved(new IdValueInput<int?>() { Id = leaseHaulerRequest.Id, Value = newApprovedValue });

            // Assert
            var updatedLeaseHaulerRequest = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.FindAsync(leaseHaulerRequest.Id));
            updatedLeaseHaulerRequest.Approved.ShouldBe(newApprovedValue);
            updatedLeaseHaulerRequest.Available.ShouldBe(leaseHaulerRequest.Available);
        }

        [Theory]
        [InlineData(11)]
        public async Task Test_UpdateAvailable_should_throw_ArgumentException_when_Available_is_less_than_Approved(int approved)
        {
            // Arrange
            var date = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerRequest = await CreateLeaseHaulerRequest(date, Shift.Shift1, _officeId, leaseHauler.Id, available: 10);

            // Act, Assert
            await _leaseHaulerRequestEditAppService.UpdateApproved(new IdValueInput<int?>() { Id = leaseHaulerRequest.Id, Value = approved }).ShouldThrowAsync(typeof(ArgumentException));

        }

    }
}
