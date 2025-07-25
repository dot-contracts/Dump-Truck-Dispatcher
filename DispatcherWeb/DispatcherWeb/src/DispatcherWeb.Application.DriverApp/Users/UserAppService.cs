using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.DriverApp.Users.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Users
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class UserAppService : DispatcherWebDriverAppAppServiceBase, IUserAppService
    {
        private readonly UserStore _userStore;
        private readonly RoleStore _roleStore;

        public UserAppService(
            UserStore userStore,
            RoleStore roleStore
            )
        {
            _userStore = userStore;
            _roleStore = roleStore;
        }

        public async Task<IPagedResult<UserDto>> Get(GetInput input)
        {
            var roleIds = input.IsInAnyRole?.Any(x => !string.IsNullOrEmpty(x)) == true ? await (await _roleStore.GetQueryAsync())
                .Where(r => input.IsInAnyRole.Contains(r.Name))
                .Select(x => x.Id)
                .ToListAsync(CancellationTokenProvider.Token) : null;

            if (roleIds != null && !roleIds.Any())
            {
                throw new UserFriendlyException("No roles matching your request (" + string.Join(", ", input.IsInAnyRole) + ") were found");
            }

            var query = (await _userStore.GetQueryAsync())
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .WhereIf(roleIds != null, x => x.Roles.Any(r => roleIds.Contains(r.RoleId)))
                .WhereIf(input.ModifiedAfterDateTime.HasValue, d => d.CreationTime > input.ModifiedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.ModifiedAfterDateTime.Value))
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    //Commented out for #13244. We'll need to send a smaller image to the driver app later, and make sure the driver app caches it.
                    //ProfilePictureId = x.ProfilePictureId,
                    LastModifiedDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                });

            var totalCount = await query.CountAsync(CancellationTokenProvider.Token);
            var items = await query
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .PageBy(input)
                .ToListAsync(CancellationTokenProvider.Token);

            return new PagedResultDto<UserDto>(
                totalCount,
                items);
        }
    }
}
