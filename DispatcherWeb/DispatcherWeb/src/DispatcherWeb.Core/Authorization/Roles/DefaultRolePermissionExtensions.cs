using Abp.Zero.Configuration;

namespace DispatcherWeb.Authorization.Roles
{
    public static class DefaultRolePermissionExtensions
    {
        public static StaticRoleDefinition SetDefaultPermissions(this StaticRoleDefinition staticRoleDefinition)
        {
            var permissions = DefaultRolePermissions.GetRolePermissions(staticRoleDefinition.RoleName);
            staticRoleDefinition.GrantedPermissions.AddRange(permissions);

            return staticRoleDefinition;
        }
    }
}
