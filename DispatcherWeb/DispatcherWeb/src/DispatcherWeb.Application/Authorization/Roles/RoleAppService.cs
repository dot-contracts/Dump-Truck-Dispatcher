using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Permissions.Dto;
using DispatcherWeb.Authorization.Roles.Dto;
using DispatcherWeb.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Application service that is used by 'role management' page.
    /// </summary>
    [AbpAuthorize(AppPermissions.Pages_Administration_Roles)]
    public class RoleAppService : DispatcherWebAppServiceBase, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly IRoleManagementConfig _roleManagementConfig;

        public RoleAppService(
            RoleManager roleManager,
            IRoleManagementConfig roleManagementConfig)
        {
            _roleManager = roleManager;
            _roleManagementConfig = roleManagementConfig;
        }

        [HttpPost]
        public async Task<PagedResultDto<RoleListDto>> GetRoles(GetRolesInput input)
        {
            var query = await GetRolesFilteredQueryAsync(input);

            var roleCount = await query.CountAsync();

            var roles = query
                .OrderBy(input.Sorting)
                .PageBy(input);

            var rolesListDtos = await GetRoleListDtoList(roles);

            return new PagedResultDto<RoleListDto>(
                roleCount,
                rolesListDtos);
        }

        [HttpPost]
        public async Task<ListResultDto<RoleListDto>> GetRolesForDropdown()
        {
            var query = await _roleManager.GetQueryAsync();

            var rolesListDtos = await GetRoleListDtoList(query);

            return new ListResultDto<RoleListDto>(rolesListDtos);
        }

        public async Task<List<SelectListDto>> GetStaticRoleNamesSelectListAsync(GetStaticRoleNamesSelectListInput input)
        {
            var multitenancySide = input.MultiTenancySide ?? await AbpSession.GetMultiTenancySideAsync();
            var staticRoleNames = _roleManagementConfig.StaticRoles
                .Where(r => r.Side == multitenancySide)
                .Select(r => new SelectListDto
                {
                    Id = r.RoleName,
                    Name = r.RoleDisplayName,
                })
                .OrderBy(x => x.Name)
                .ToList();

            return staticRoleNames;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Create, AppPermissions.Pages_Administration_Roles_Edit)]
        public async Task<GetRoleForEditOutput> GetRoleForEdit(NullableIdDto input)
        {
            var permissions = (await PermissionManager.GetAllPermissionsAsync())
                .Select(x => new FlatPermissionDto
                {
                    ParentName = x.Parent?.Name,
                    Name = x.Name,
                    DisplayName = L(x.DisplayName),
                })
                .OrderBy(p => p.DisplayName)
                .ToList();

            var grantedPermissions = Array.Empty<Permission>();
            RoleEditDto roleEditDto;

            if (input.Id.HasValue) //Editing existing role?
            {
                var role = await _roleManager.GetRoleByIdAsync(input.Id.Value);
                grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
                roleEditDto = new RoleEditDto
                {
                    Id = role.Id,
                    DisplayName = role.DisplayName,
                    IsDefault = role.IsDefault,
                };
            }
            else
            {
                roleEditDto = new RoleEditDto();
            }

            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = permissions,
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList(),
            };
        }

        public async Task CreateOrUpdateRole(CreateOrUpdateRoleInput input)
        {
            if (input.Role.Id.HasValue)
            {
                await UpdateRoleAsync(input);
            }
            else
            {
                await CreateRoleAsync(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Delete)]
        public async Task<bool> IsRoleAssignedToUsers(EntityDto input)
        {
            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            var users = await UserManager.GetUsersInRoleAsync(role.Name);

            return users.Any();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Delete)]
        public async Task DeleteRole(EntityDto input)
        {
            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            var users = await UserManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in users)
            {
                CheckErrors(await UserManager.RemoveFromRoleAsync(user, role.Name));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        public async Task RestoreDefaultPermissionsAsync(int roleId)
        {
            var role = await _roleManager.GetRoleByIdAsync(roleId);
            await _roleManager.RestoreDefaultPermissionsAsync(role);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Edit)]
        protected virtual async Task UpdateRoleAsync(CreateOrUpdateRoleInput input)
        {
            Debug.Assert(input.Role.Id != null, "input.Role.Id should be set.");

            var role = await _roleManager.GetRoleByIdAsync(input.Role.Id.Value);
            role.DisplayName = input.Role.DisplayName;
            role.IsDefault = input.Role.IsDefault;

            await UpdateGrantedPermissionsAsync(role, input.GrantedPermissionNames);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Roles_Create)]
        protected virtual async Task CreateRoleAsync(CreateOrUpdateRoleInput input)
        {
            var role = new Role(await AbpSession.GetTenantIdOrNullAsync(), input.Role.DisplayName) { IsDefault = input.Role.IsDefault };
            CheckErrors(await _roleManager.CreateAsync(role));
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
            await UpdateGrantedPermissionsAsync(role, input.GrantedPermissionNames);
        }

        private async Task UpdateGrantedPermissionsAsync(Role role, List<string> grantedPermissionNames)
        {
            var grantedPermissions = PermissionManager.GetPermissionsFromNamesByValidating(grantedPermissionNames);
            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
        }

        private static async Task<List<RoleListDto>> GetRoleListDtoList(IQueryable<Role> roles)
        {
            var data = await (from r in roles
                              select new RoleListDto
                              {
                                  Id = r.Id,
                                  Name = r.Name,
                                  DisplayName = r.DisplayName,
                                  IsStatic = r.IsStatic,
                                  IsDefault = r.IsDefault,
                                  CreationTime = r.CreationTime,
                              }).ToListAsync();


            var roleListDtos = data;
            return roleListDtos;
        }

        private async Task<IQueryable<Role>> GetRolesFilteredQueryAsync(IGetRolesInput input)
        {
            var query = await _roleManager.GetAvailableRolesAsync();
            var multiTenancySide = await AbpSession.GetMultiTenancySideAsync();

            if (!string.IsNullOrEmpty(input.Permission))
            {
                var roleNamesWithPermissionGrantedByDefault = _roleManagementConfig.StaticRoles.Where(
                    r => (r.GrantAllPermissionsByDefault || DefaultRolePermissions.IsPermissionsGrantedToRole(r.RoleName, input.Permission))
                        && r.Side == multiTenancySide
                ).Select(r => r.RoleName).ToList();

                query = query.Where(r =>
                    r.Permissions.Any(rp => rp.Name == input.Permission)
                        ? r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                        : roleNamesWithPermissionGrantedByDefault.Contains(r.Name)
                );
            }

            return query;
        }
    }
}
