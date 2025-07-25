using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Role manager.
    /// Used to implement domain logic for roles.
    /// </summary>
    public class RoleManager : AbpRoleManager<Role, User>
    {
        private readonly IPermissionManager _permissionManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IFeatureChecker _featureChecker;

        public RoleManager(
            RoleStore store,
            IEnumerable<IRoleValidator<Role>> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<RoleManager> logger,
            IPermissionManager permissionManager,
            IRoleManagementConfig roleManagementConfig,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILocalizationManager localizationManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IFeatureChecker featureChecker)
            : base(
                store,
                roleValidators,
                keyNormalizer,
                errors,
                logger,
                permissionManager,
                cacheManager,
                unitOfWorkManager,
                roleManagementConfig,
                organizationUnitRepository,
                organizationUnitRoleRepository)
        {
            _localizationManager = localizationManager;
            _permissionManager = permissionManager;
            _featureChecker = featureChecker;
        }

        public override Task SetGrantedPermissionsAsync(Role role, IEnumerable<Permission> permissions)
        {
            var permissionList = permissions as List<Permission> ?? permissions.ToList();

            CheckPermissionsToUpdate(role, permissionList);

            return base.SetGrantedPermissionsAsync(role, permissionList);
        }

        public virtual async Task<Role> GetRoleByIdAsync(long roleId)
        {
            var role = await FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new ApplicationException("There is no role with id: " + roleId);
            }

            return role;
        }

        private void CheckPermissionsToUpdate(Role role, List<Permission> permissions)
        {
            if (role.Name == StaticRoleNames.Host.Admin
                && (!permissions.Any(p => p.Name == AppPermissions.Pages_Administration_Roles_Edit)
                 || !permissions.Any(p => p.Name == AppPermissions.Pages_Administration_Users_ChangePermissions)))
            {
                throw new UserFriendlyException(L("YouCannotRemoveUserRolePermissionsFromAdminRole"));
            }
        }

        private new string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        public async Task RestoreDefaultPermissionsAsync(Role role)
        {
            foreach (string permissionName in DefaultRolePermissions.DefaultPermissions)
            {
                if (DefaultRolePermissions.IsPermissionsGrantedToRole(role.Name, permissionName))
                {
                    await GrantPermissionToRoleAsync(role, permissionName);
                }
                else
                {
                    await RevokePermissionFromRoleAsync(role, permissionName);
                }
            }
        }
        private async Task GrantPermissionToRoleAsync(Role role, string permissionName)
        {
            var permission = _permissionManager.GetPermission(permissionName);
            await GrantPermissionAsync(role, permission);
        }

        private async Task RevokePermissionFromRoleAsync(Role role, string permissionName)
        {
            var permission = _permissionManager.GetPermission(permissionName);
            await ProhibitPermissionAsync(role, permission);
        }

        public async Task<IQueryable<Role>> GetAvailableRolesAsync()
        {
            if (await AbpSession.GetMultiTenancySideAsync() == MultiTenancySides.Tenant && AbpSession.UserId.HasValue)
            {
                return (await GetQueryAsync())
                    .WhereIf(!await _featureChecker.IsEnabledAsync(AppFeatures.CustomerPortal), x => x.Name != StaticRoleNames.Tenants.Customer)
                    .WhereIf(!await _featureChecker.IsEnabledAsync(AppFeatures.LeaseHaulerPortal), x => x.Name != StaticRoleNames.Tenants.LeaseHaulerAdministrator);
            }
            return await GetQueryAsync();
        }
    }
}
