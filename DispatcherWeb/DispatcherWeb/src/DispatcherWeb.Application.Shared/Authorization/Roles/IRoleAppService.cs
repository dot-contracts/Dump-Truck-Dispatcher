using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Authorization.Roles.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Application service that is used by 'role management' page.
    /// </summary>
    public interface IRoleAppService : IApplicationService
    {
        Task<PagedResultDto<RoleListDto>> GetRoles(GetRolesInput input);
        Task<ListResultDto<RoleListDto>> GetRolesForDropdown();
        Task<GetRoleForEditOutput> GetRoleForEdit(NullableIdDto input);
        Task CreateOrUpdateRole(CreateOrUpdateRoleInput input);
        Task<bool> IsRoleAssignedToUsers(EntityDto input);
        Task DeleteRole(EntityDto input);
        Task RestoreDefaultPermissionsAsync(int roleId);
        Task<List<SelectListDto>> GetStaticRoleNamesSelectListAsync(GetStaticRoleNamesSelectListInput input);
    }
}
