using System;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestEditAppService_EditLeaseHaulerRequest_Tests : LeaseHaulerRequestEditAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHaulerRequest_should_Create_LeaseHaulerRequest()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var model = new LeaseHaulerRequestEditModel
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                LeaseHaulerId = leaseHauler.Id,
                Available = 10,
                Approved = 5,
            };

            // Act
            var result = await _leaseHaulerRequestEditAppService.EditLeaseHaulerRequest(model);

            // Assert 
            result.Id.ShouldNotBe(0);

            var leaseHaulerRequests = await UsingDbContextAsync(async context => await context.LeaseHaulerRequests.ToListAsync());
            leaseHaulerRequests.Count.ShouldBe(1);
            var createdLeaseHaulerRequest = leaseHaulerRequests[0];
            createdLeaseHaulerRequest.OfficeId.ShouldBe(_officeId);
            createdLeaseHaulerRequest.Sent.ShouldBeNull();
            createdLeaseHaulerRequest.Date.ShouldBe(model.Date);
            createdLeaseHaulerRequest.Shift.ShouldBe(model.Shift);
            createdLeaseHaulerRequest.LeaseHaulerId.ShouldBe(model.LeaseHaulerId);
            createdLeaseHaulerRequest.Available.ShouldBe(model.Available);
            createdLeaseHaulerRequest.Approved.ShouldBe(model.Approved);
        }

        [Theory]
        [InlineData(3, 4)]
        public async Task Test_EditLeaseHaulerRequest_should_throw_ArgumentException_when_Approved_is_equal_or_greater_Available(int available, int approved)
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var model = new LeaseHaulerRequestEditModel
            {
                Date = Clock.Now.Date,
                Shift = Shift.Shift1,
                LeaseHaulerId = leaseHauler.Id,
                Available = available,
                Approved = approved,
            };

            // Act, Assert
            await _leaseHaulerRequestEditAppService.EditLeaseHaulerRequest(model).ShouldThrowAsync(typeof(ArgumentException));
        }
    }
}
