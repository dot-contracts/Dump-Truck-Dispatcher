using System.Threading.Tasks;
using Abp.Authorization.Users;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Authorization
{
    public static class UserManagerExtensions
    {
        public static async Task<User> GetAdminAsync(this UserManager userManager)
        {
            return await userManager.FindByNameAsync(AbpUserBase.AdminUserName);
        }
    }
}
