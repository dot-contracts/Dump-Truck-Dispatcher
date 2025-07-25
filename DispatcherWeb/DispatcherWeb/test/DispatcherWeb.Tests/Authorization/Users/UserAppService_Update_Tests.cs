using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Drivers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Authorization.Users
{
    public class UserAppService_Update_Tests : UserAppServiceTestBase
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Update_User_Basic_Tests()
        {
            //Arrange
            var managerRole = CreateRole("Manager");
            var adminUser = await GetUserByUserNameOrNullAsync(User.AdminUserName);

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        Id = adminUser.Id,
                        EmailAddress = "admin1@abp.com",
                        Name = "System1",
                        Surname = "Admin2",
                        Password = "123qwE*",
                        UserName = adminUser.UserName,
                    },
                    AssignedRoleNames = new[] { "Manager" },
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                //Get created user
                var updatedAdminUser = await GetUserByUserNameOrNullAsync(adminUser.UserName, includeRoles: true);
                updatedAdminUser.ShouldNotBe(null);
                updatedAdminUser.Id.ShouldBe(adminUser.Id);

                //Check some properties
                updatedAdminUser.EmailAddress.ShouldBe("admin1@abp.com");
                updatedAdminUser.TenantId.ShouldBe(Session.TenantId);

                LocalIocManager
                    .Resolve<IPasswordHasher<User>>()
                    .VerifyHashedPassword(updatedAdminUser, updatedAdminUser.Password, "123qwE*")
                    .ShouldBe(PasswordVerificationResult.Success);

                //Check roles
                updatedAdminUser.Roles.Count.ShouldBe(1);
                updatedAdminUser.Roles.Any(ur => ur.RoleId == managerRole.Id).ShouldBe(true);
            });
        }

        [Fact]
        public async Task Should_Not_Update_User_With_Duplicate_Username_Or_EmailAddress()
        {
            //Arrange

            await CreateTestUsersAsync();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");

            //Act

            //Try to update with existing username
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                await UserAppService.CreateOrUpdateUser(
                    new CreateOrUpdateUserInput
                    {
                        User = new UserEditDto
                        {
                            Id = jnashUser.Id,
                            EmailAddress = "jnsh2000@testdomain.com",
                            Name = "John",
                            Surname = "Nash",
                            UserName = "adams_d", //Changed user name to an existing user
                            Password = "123qwE*",
                        },
                        AssignedRoleNames = new string[0],
                    }));

            exception.Message.ShouldContain("adams_d");

            //Try to update with existing email address
            exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                await UserAppService.CreateOrUpdateUser(
                    new CreateOrUpdateUserInput
                    {
                        User = new UserEditDto
                        {
                            Id = jnashUser.Id,
                            EmailAddress = "adams_d@gmail.com", //Changed email to an existing user
                            Name = "John",
                            Surname = "Nash",
                            UserName = "jnash",
                            Password = "123qwE*",
                        },
                        AssignedRoleNames = new string[0],
                    }));

            exception.Message.ShouldContain("adams_d@gmail.com");
        }

        [MultiTenantFact]
        public async Task Should_Remove_From_Role()
        {
            LoginAsHostAdmin();

            //Arrange
            var adminUser = await GetUserByUserNameOrNullAsync(User.AdminUserName);
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBeGreaterThan(0); //There should be 1 role at least
            });

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto //Not changing user properties
                    {
                        Id = adminUser.Id,
                        EmailAddress = adminUser.EmailAddress,
                        Name = adminUser.Name,
                        Surname = adminUser.Surname,
                        UserName = adminUser.UserName,
                        Password = null,
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Host.Admin }, //Just deleting all roles expect admin
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBe(1);
            });
        }

        [Fact]
        public async Task Test_CreateOrUpdateUser_should_update_Driver_UserId_when_email_match_to_driver_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            await CreateTestUsersAsync();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);
            var driver = await CreateDriver(email: "driver1@example.com");

            // Act
            await UserAppService.CreateOrUpdateUser(new CreateOrUpdateUserInput
            {
                User = new UserEditDto
                {
                    Id = jnashUser.Id,
                    EmailAddress = driver.EmailAddress,
                    Name = "John",
                    Surname = "Nash",
                    UserName = "driver1",
                    Password = "123qwe",
                },
                AssignedRoleNames = new string[] { StaticRoleNames.Tenants.Driver },
            });

            // Assert
            var updatedDriver = await UsingDbContextAsync(async context => await context.Drivers.FindAsync(driver.Id));
            updatedDriver.UserId.ShouldNotBeNull();
        }

        [Fact]
        public async Task Test_CreateOrUpdateUser_should_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_associated_with_Driver_role_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            await CreateTestUsersAsync();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);

            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput(jnashUser.Id, new[] { StaticRoleNames.Tenants.Driver }));

            // Assert
            await driverUserLinkService.ReceivedWithAnyArgs().UpdateDriver(null);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateOrUpdateUser_should_not_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_not_associated_with_Driver_role_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            await CreateTestUsersAsync();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);

            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput(jnashUser.Id));

            // Assert
            await driverUserLinkService.DidNotReceiveWithAnyArgs().UpdateDriver(null);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateOrUpdateUser_should_not_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_associated_with_Driver_role_and_DispatchVia_is_not_DriverApplication()
        {
            // Arrange
            await CreateTestUsersAsync();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);

            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput(jnashUser.Id, new[] { StaticRoleNames.Tenants.Driver }));

            // Assert
            await driverUserLinkService.DidNotReceiveWithAnyArgs().UpdateDriver(null);
        }

        protected Role CreateRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Add(new Role(Session.TenantId, roleName, roleName)).Entity);
        }
    }
}
