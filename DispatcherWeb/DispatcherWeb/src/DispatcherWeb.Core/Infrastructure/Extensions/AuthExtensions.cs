using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb
{
    public static class AuthExtensions
    {
        public static async Task<IQueryable<User>> GetUsersWithGrantedPermission(this UserManager userManager, RoleManager roleManager, string permissionName)
        {
            return await GetUsersWithGrantedPermission(await userManager.GetQueryAsync(), roleManager, permissionName);
        }

        public static async Task<IQueryable<User>> GetUsersWithGrantedPermission(this IQueryable<User> query, RoleManager roleManager, string permissionName)
        {
            var roleNamesWithDefaultPermission = DefaultRolePermissions.GetRoleNamesHavingDefaultPermission(permissionName);

            var roleIdsWithGrantedPermissions = await (await roleManager.GetAvailableRolesAsync())
                .Where(x => !x.Permissions.Any(p => p.Name == permissionName && !p.IsGranted)
                            && (roleNamesWithDefaultPermission.Contains(x.Name) || x.Permissions.Any(p => p.Name == permissionName && p.IsGranted)))
                .Select(x => x.Id)
                .ToListAsync();

            query = query
                .Where(x => !x.Permissions.Any(p => p.Name == permissionName && !p.IsGranted)
                            && (x.Roles.Any(r => roleIdsWithGrantedPermissions.Contains(r.RoleId)) || x.Permissions.Any(p => p.Name == permissionName && p.IsGranted)));

            return query;
        }
    }
}
