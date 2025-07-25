using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.DriverAssignments.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class DriverAssignmentAppService_EditDriverAssignment_Tests : DriverAssignmentAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditDriverAssignment_should_throw_UserFriendlyException_when_setting_DriverId_null_and_OrderLineTruck_exists()
        {
            // Arrange
            var driverAssignment = await CreateTestDriverAssignment();
            var input = new DriverAssignmentEditDto
            {
                Id = driverAssignment.Id,
                DriverId = null,
            };

            // Act, Assert
            await _driverAssignmentAppService.EditDriverAssignment(input).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditDriverAssignment_should_throw_UserFriendlyException_when_changing_DriverId_and_OrderLineTruck_exists()
        {
            // Arrange
            var driverAssignment = await CreateTestDriverAssignment();
            var driver2 = await CreateDriver(officeId: _officeId);
            var input = new DriverAssignmentEditDto
            {
                Id = driverAssignment.Id,
                DriverId = driver2.Id,
            };

            // Act, Assert
            await _driverAssignmentAppService.EditDriverAssignment(input).ShouldThrowAsync(typeof(UserFriendlyException));
        }
    }
}
