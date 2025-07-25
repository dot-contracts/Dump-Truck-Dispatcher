using Abp.MultiTenancy;
using Abp.Zero.Configuration;

namespace DispatcherWeb.Authorization.Roles
{
    public static class AppRoleConfig
    {
        public static void Configure(IRoleManagementConfig roleManagementConfig)
        {
            //Static host roles

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Host.Admin,
                    MultiTenancySides.Host,
                    grantAllPermissionsByDefault: true)
                );

            //Static tenant roles
            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.User,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Admin,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Dispatching,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.LimitedQuoting,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Quoting,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Backoffice,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Administrative,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Managers,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Maintenance,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.MaintenanceSupervisor,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Driver,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.LeaseHaulerDriver,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Customer,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );

            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    MultiTenancySides.Tenant)
                .SetDefaultPermissions()
                );
        }
    }
}
