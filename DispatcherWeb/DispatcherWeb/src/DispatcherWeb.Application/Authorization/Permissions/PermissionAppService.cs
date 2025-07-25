using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using DispatcherWeb.Authorization.Permissions.Dto;

namespace DispatcherWeb.Authorization.Permissions
{
    [AbpAuthorize]
    public class PermissionAppService : DispatcherWebAppServiceBase, IPermissionAppService
    {
        public async Task<IListResult<string>> GetGrantedPermissions()
        {
            var user = await GetCurrentUserAsync();
            var grantedPermissions = await UserManager.GetGrantedPermissionsAsync(user);
            return new ListResultDto<string>(grantedPermissions.Select(x => x.Name).ToList());
        }

        public async Task<ListResultDto<FlatPermissionWithLevelDto>> GetAllPermissionsAsync()
        {
            var permissions = await PermissionManager.GetAllPermissionsAsync();
            var rootPermissions = permissions.Where(p => p.Parent == null);

            var result = new List<FlatPermissionWithLevelDto>();

            foreach (var rootPermission in rootPermissions)
            {
                var level = 0;
                AddPermission(rootPermission, permissions, result, level);
            }

            return new ListResultDto<FlatPermissionWithLevelDto>
            {
                Items = result,
            };
        }

        private void AddPermission(Permission permission, IReadOnlyList<Permission> allPermissions, List<FlatPermissionWithLevelDto> result, int level)
        {
            var flatPermission = new FlatPermissionWithLevelDto
            {
                Level = level,
                ParentName = permission.Parent?.Name,
                Name = permission.Name,
                DisplayName = L(permission.DisplayName),
            };
            result.Add(flatPermission);

            if (permission.Children == null)
            {
                return;
            }

            var children = allPermissions.Where(p => p.Parent != null && p.Parent.Name == permission.Name).ToList();

            foreach (var childPermission in children)
            {
                AddPermission(childPermission, allPermissions, result, level + 1);
            }
        }
    }
}
