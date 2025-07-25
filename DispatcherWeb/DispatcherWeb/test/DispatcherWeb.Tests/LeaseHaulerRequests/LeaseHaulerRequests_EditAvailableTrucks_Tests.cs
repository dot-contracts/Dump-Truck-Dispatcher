using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.LeaseHaulerRequests;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequests_EditAvailableTrucks_Tests : LeaseHaulerRequestEditAppService_Tests_Base
    {
        // #8604 Commented until further notice
        //[Fact]
        //public async Task Test_EditAvailableTrucks_should_throw_UserFriendlyException_when_Available_changed()
        //{
        //    // Arrange
        //    var request = await LeaseHaulerRequest(2, null);
        //    var model = new AvailableTrucksEditModel()
        //    {
        //        Id = request.Guid.ToShortGuid(),
        //        Available = 3,
        //    };

        //    // Act, Assert
        //    await _leaseHaulerRequestEditAppService.EditAvailableTrucks(model).ShouldThrowAsync(typeof(UserFriendlyException));
        //}

        //[Fact]
        //public async Task Test_EditAvailableTrucks_should_throw_UserFriendlyException_when_Approved_changed()
        //{
        //    // Arrange
        //    var request = await LeaseHaulerRequest(2, 1);
        //    var model = new AvailableTrucksEditModel()
        //    {
        //        Id = request.Guid.ToShortGuid(),
        //        Available = request.Available ?? 0,
        //        Approved = null,

        //        Trucks = new List<int?> { 1 },
        //        Drivers = new List<int?> { 1 },
        //    };

        //    // Act, Assert
        //    await _leaseHaulerRequestEditAppService.EditAvailableTrucks(model).ShouldThrowAsync(typeof(UserFriendlyException));
        //}

        private async Task<LeaseHaulerRequest> LeaseHaulerRequest(int? available, int? approved)
        {
            var date = Clock.Now.Date;
            var leaseHauler = await CreateLeaseHauler();
            var request = await CreateLeaseHaulerRequest(leaseHauler.Id, date, Shift.Shift1, available, approved);
            return request;
        }
    }
}
