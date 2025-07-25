using System.Threading.Tasks;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Drivers
{
    public class DriverUserLinkService_Tests : AppTestBase, IAsyncLifetime
    {
        private IDriverUserLinkService _driverUserLinkService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;

            _driverUserLinkService = Resolve<IDriverUserLinkService>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_UpdateDriver_should_update_UserId_when_driver_exists()
        {
            // Arrange
            var user = await CreateUser(1, true);
            var driver = await CreateDriver(email: user.EmailAddress);

            // Act
            await _driverUserLinkService.UpdateDriver(user);

            // Assert
            var updatedDriver = await UsingDbContextAsync(async context => await context.Drivers.FindAsync(driver.Id));
            updatedDriver.UserId.ShouldBe(user.Id);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_UpdateDriver_should_create_new_driver_when_driver_does_not_exist()
        {
            // Arrange
            var user = await CreateUser(1, true);

            // Act
            await _driverUserLinkService.UpdateDriver(user);

            // Assert
            var updatedDriver = await UsingDbContextAsync(async context => await context.Drivers.FirstOrDefaultAsync());
            updatedDriver.ShouldNotBeNull();
            updatedDriver.TenantId.ShouldBe(1);
            updatedDriver.UserId.ShouldBe(user.Id);
            updatedDriver.OfficeId.ShouldBe(user.OfficeId);
            updatedDriver.FirstName.ShouldBe(user.Name);
            updatedDriver.LastName.ShouldBe(user.Surname);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
