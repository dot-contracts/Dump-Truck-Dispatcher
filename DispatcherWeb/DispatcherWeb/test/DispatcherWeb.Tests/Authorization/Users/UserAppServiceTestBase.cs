using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.EntityFrameworkCore.Uow;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.EntityFrameworkCore;

namespace DispatcherWeb.Tests.Authorization.Users
{
    public abstract class UserAppServiceTestBase : AppTestBase
    {
        protected readonly IUserAppService UserAppService;

        protected UserAppServiceTestBase()
        {
            UserAppService = Resolve<IUserAppService>();
            SubstituteServiceDependencies(UserAppService);
        }

        protected async Task CreateTestUsersAsync()
        {
            //Note: There is a default "admin" user also

            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                context.Users.Add(CreateUserEntity("jnash", "John", "Nash", "jnsh2000@testdomain.com"));
                context.Users.Add(CreateUserEntity("adams_d", "Douglas", "Adams", "adams_d@gmail.com"));
                context.Users.Add(CreateUserEntity("artdent", "Arthur", "Dent", "ArthurDent@yahoo.com"));
            });
        }

        protected User CreateUserEntity(string userName, string name, string surname, string emailAddress)
        {
            var user = new User
            {
                EmailAddress = emailAddress,
                IsEmailConfirmed = true,
                Name = name,
                Surname = surname,
                UserName = userName,
                Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==", //123qwe
                TenantId = Session.TenantId,
                OfficeId = 1,
                Permissions = new List<UserPermissionSetting>
                {
                    new UserPermissionSetting {Name = "test.permission1", IsGranted = true, TenantId = Session.TenantId},
                    new UserPermissionSetting {Name = "test.permission2", IsGranted = true, TenantId = Session.TenantId},
                    new UserPermissionSetting {Name = "test.permission3", IsGranted = false, TenantId = Session.TenantId},
                    new UserPermissionSetting {Name = "test.permission4", IsGranted = false, TenantId = Session.TenantId},
                },
            };

            user.SetNormalizedNames();

            return user;
        }

        protected CreateOrUpdateUserInput GetCreateOrUpdateUserInput(long? userId = null, string[] roles = null) =>
            new CreateOrUpdateUserInput
            {
                User = new UserEditDto
                {
                    Id = userId,
                    EmailAddress = "test@example.com",
                    Name = "Test",
                    Surname = "Test",
                    UserName = "test",
                    Password = "123qwe",
                    OfficeId = 1,
                },
                AssignedRoleNames = roles ?? new string[0],
            };
    }
}
