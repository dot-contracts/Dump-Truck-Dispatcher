using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulers
{
    public class LeaseHaulerAppService_EditLeaseHauler_Tests : LeaseHaulerAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHauler_should_create_LeaseHauler()
        {
            // Arrange
            var model = new LeaseHaulerEditDto
            {
                Name = "LeaseHauler1",
            };

            // Act
            var result = await _leaseHaulerAppService.EditLeaseHauler(model);

            // Assert
            result.ShouldNotBe(0);
            var leaseHaulers = await UsingDbContextAsync(async context => await context.LeaseHaulers.ToListAsync());
            leaseHaulers.Count.ShouldBe(1);
            leaseHaulers[0].Id.ShouldBe(result);
            leaseHaulers[0].Name.ShouldBe(model.Name);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHauler_should_edit_LeaseHauler()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler("LH1");
            var model = new LeaseHaulerEditDto
            {
                Id = leaseHauler.Id,
                Name = "LeaseHauler1",
            };

            // Act
            var result = await _leaseHaulerAppService.EditLeaseHauler(model);

            // Assert
            result.ShouldNotBe(0);
            var leaseHaulers = await UsingDbContextAsync(async context => await context.LeaseHaulers.ToListAsync());
            leaseHaulers.Count.ShouldBe(1);
            leaseHaulers[0].Id.ShouldBe(result);
            leaseHaulers[0].Name.ShouldBe(model.Name);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHauler_should_throw_UserFriendlyException_when_creating_LeaseHauler_and_another_one_exists_with_same_name()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler();
            var model = new LeaseHaulerEditDto
            {
                Id = null,
                Name = leaseHauler.Name,
            };

            // Act, Assert
            await _leaseHaulerAppService.EditLeaseHauler(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditLeaseHauler_should_throw_UserFriendlyException_when_editing_LeaseHauler_and_another_one_exists_with_same_name()
        {
            // Arrange
            var leaseHauler = await CreateLeaseHauler("LH1");
            var leaseHauler2 = await CreateLeaseHauler();
            var model = new LeaseHaulerEditDto
            {
                Id = leaseHauler.Id,
                Name = leaseHauler2.Name,
            };

            // Act, Assert
            await _leaseHaulerAppService.EditLeaseHauler(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }

    }
}
