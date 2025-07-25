using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Authorization.Users
{
    public class UserAppService_Create_Tests : UserAppServiceTestBase
    {
        [MultiTenantFact]
        public async Task Should_Create_User_For_Host()
        {
            LoginAsHostAdmin();

            await CreateUserAndTestAsync("jnash", "John", "Nash", "jnsh2000@testdomain.com", null);
            await CreateUserAndTestAsync("adams_d", "Douglas", "Adams", "adams_d@gmail.com", null, StaticRoleNames.Host.Admin);
        }

        [Fact]
        public async Task Should_Create_User_For_Tenant()
        {
            var defaultTenantId = (await GetTenantAsync(Tenant.DefaultTenantName)).Id;
            await CreateUserAndTestAsync("jnash", "John", "Nash", "jnsh2000@testdomain.com", defaultTenantId);
            await CreateUserAndTestAsync("adams_d", "Douglas", "Adams", "adams_d@gmail.com", defaultTenantId, StaticRoleNames.Tenants.Admin);
        }

        [Fact]
        public async Task Should_Not_Create_User_With_Duplicate_Username_Or_EmailAddress()
        {
            //Arrange
            await CreateTestUsersAsync();

            //Act
            await Assert.ThrowsAsync<UserFriendlyException>(
                async () =>
                    await UserAppService.CreateOrUpdateUser(
                        new CreateOrUpdateUserInput
                        {
                            User = new UserEditDto
                            {
                                EmailAddress = "john@nash.com",
                                Name = "John",
                                Surname = "Nash",
                                UserName = "jnash", //Same username is added before (in CreateTestUsers)
                                Password = "123qwe",
                            },
                            AssignedRoleNames = new string[0],
                        }));
        }

        [Fact]
        public async Task Test_CreateOrUpdateUser_should_update_Driver_UserId_when_email_match_to_driver_and_DispatchViaDriverApplication_is_true()
        {
            // Arrange
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);
            var driver = await CreateDriver(email: "driver1@example.com");

            // Act
            await UserAppService.CreateOrUpdateUser(new CreateOrUpdateUserInput
            {
                User = new UserEditDto
                {
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
        public async Task Test_CreateOrUpdateUser_should_create_Driver_when_User_is_in_Driver_role_and_DispatchViaDriverApplication_is_true()
        {
            // Arrange
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);
            var userEditDto = new UserEditDto
            {
                EmailAddress = "d1@example.com",
                Name = "John",
                Surname = "Nash",
                UserName = "driver1",
                Password = "123qwe",
                OfficeId = 1,
            };

            // Act
            await UserAppService.CreateOrUpdateUser(new CreateOrUpdateUserInput
            {
                User = userEditDto,
                AssignedRoleNames = new string[] { StaticRoleNames.Tenants.Driver },
            });

            // Assert
            var createdUser = await UsingDbContextAsync(async context => await context.Users.FirstOrDefaultAsync(u => u.UserName == userEditDto.UserName));
            createdUser.ShouldNotBeNull();
            var createdDriver = await UsingDbContextAsync(async context => await context.Drivers.FirstOrDefaultAsync(d => d.UserId == createdUser.Id));
            createdDriver.ShouldNotBeNull();
        }

        [Fact]
        public async Task Test_CreateOrUpdateUser_should_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_associated_with_Driver_role_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);


            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput(roles: new[] { StaticRoleNames.Tenants.Driver }));

            // Assert
            await driverUserLinkService.ReceivedWithAnyArgs().UpdateDriver(null);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateOrUpdateUser_should_not_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_not_associated_with_Driver_role_and_DispatchVia_is_DriverApplication()
        {
            // Arrange
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.DriverApplication);

            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput());

            // Assert
            await driverUserLinkService.DidNotReceiveWithAnyArgs().UpdateDriver(null);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateOrUpdateUser_should_not_call_UpdateDriverUserIdOrCreateNewDriver_when_User_is_associated_with_Driver_role_and_DispatchVia_is_not_DriverApplication()
        {
            // Arrange
            var driverUserLinkService = Substitute.For<IDriverUserLinkService>();
            var userAppService = Resolve<IUserAppService>(new { driverUserLinkService });
            SubstituteServiceDependencies(userAppService);
            SubstituteDispatchViaSetting(DispatchVia.None);

            // Act
            await userAppService.CreateOrUpdateUser(GetCreateOrUpdateUserInput(roles: new[] { StaticRoleNames.Tenants.Driver }));

            // Assert
            await driverUserLinkService.DidNotReceiveWithAnyArgs().UpdateDriver(null);
        }

        private async Task CreateUserAndTestAsync(string userName, string name, string surname, string emailAddress, int? tenantId, params string[] roleNames)
        {
            //Arrange
            Session.TenantId = tenantId;

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        EmailAddress = emailAddress,
                        Name = name,
                        Surname = surname,
                        UserName = userName,
                        Password = "123qwE*",
                    },
                    AssignedRoleNames = roleNames,
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                //Get created user
                var createdUser = await context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserName == userName);
                createdUser.ShouldNotBe(null);

                //Check some properties
                createdUser.EmailAddress.ShouldBe(emailAddress);
                createdUser.TenantId.ShouldBe(tenantId);

                //Check roles
                if (roleNames.IsNullOrEmpty())
                {
                    createdUser.Roles.Count.ShouldBe(0);
                }
                else
                {
                    createdUser.Roles.Count.ShouldBe(roleNames.Length);
                    foreach (var roleName in roleNames)
                    {
                        var roleId = (await GetRoleAsync(roleName)).Id;
                        createdUser.Roles.Any(ur => ur.RoleId == roleId && ur.TenantId == tenantId).ShouldBe(true);
                    }
                }
            });
        }
    }
}
