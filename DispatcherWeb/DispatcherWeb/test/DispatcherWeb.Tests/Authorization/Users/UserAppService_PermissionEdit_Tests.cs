using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Authorization.Users
{
    public class UserAppService_PermissionEdit_Tests : UserAppServiceTestBase
    {
        [Fact]
        public async Task Should_Get_User_Permissions()
        {
            //Arrange
            var admin = await GetUserByUserNameAsync(User.AdminUserName);

            //Act
            var output = await UserAppService.GetUserPermissionsForEdit(new EntityDto<long>(admin.Id));

            //Assert
            output.GrantedPermissionNames.ShouldNotBe(null);
            output.Permissions.ShouldNotBe(null);
        }

        [Fact]
        public async Task Should_Update_User_Permissions()
        {
            //Arrange
            var multiTenancySide = await Session.GetMultiTenancySideAsync();
            var admin = await GetUserByUserNameAsync(User.AdminUserName);
            var permissions = (await Resolve<IPermissionManager>()
                    .GetAllPermissionsAsync())
                .Where(p => p.MultiTenancySides.HasFlag(multiTenancySide))
                .ToList();

            //Act
            await UserAppService.UpdateUserPermissions(
                new UpdateUserPermissionsInput
                {
                    Id = admin.Id,
                    GrantedPermissionNames = permissions.Select(p => p.Name).ToList(),
                });

            //Assert
            var userManager = Resolve<UserManager>();
            foreach (var permission in permissions)
            {
                (await userManager.IsGrantedAsync(admin, permission)).ShouldBe(true);
            }
        }

        [Fact]
        public async Task Should_Reset_Permissions()
        {
            //Arrange
            var admin = await GetUserByUserNameAsync(User.AdminUserName);
            UsingDbContext(
                context => context.UserPermissions.Add(
                    new UserPermissionSetting
                    {
                        TenantId = Session.TenantId,
                        UserId = admin.Id,
                        Name = AppPermissions.Pages_Administration_Roles,
                        IsGranted = false,
                    }));

            //Act
            await UserAppService.ResetUserSpecificPermissions(new EntityDto<long>(admin.Id));

            //Assert
            (await UsingDbContextAsync(context => context.UserPermissions.CountAsync(p => p.UserId == admin.Id))).ShouldBe(0);
        }
    }
}
