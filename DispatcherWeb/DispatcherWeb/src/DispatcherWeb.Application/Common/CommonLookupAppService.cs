using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Editions;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.Sessions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Common
{
    [AbpAuthorize]
    public class CommonLookupAppService : DispatcherWebAppServiceBase, ICommonLookupAppService
    {
        private readonly EditionManager _editionManager;
        private readonly RoleManager _roleManager;

        public CommonLookupAppService(
            EditionManager editionManager,
            RoleManager roleManager
        )
        {
            _editionManager = editionManager;
            _roleManager = roleManager;
        }

        public async Task<ListResultDto<SubscribableEditionComboboxItemDto>> GetEditionsForCombobox(bool onlyFreeItems = false)
        {
            var subscribableEditions = (await (await _editionManager.GetQueryAsync()).Cast<SubscribableEdition>().ToListAsync())
                .WhereIf(onlyFreeItems, e => e.IsFree)
                .OrderBy(e => e.MonthlyPrice);

            return new ListResultDto<SubscribableEditionComboboxItemDto>(
                subscribableEditions.Select(e => new SubscribableEditionComboboxItemDto(e.Id.ToString(), e.DisplayName, e.IsFree)).ToList()
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Users, AppPermissions.LeaseHaulerPortal_SelectLists_Users, AppPermissions.CustomerPortal_SelectLists_Users)]
        public async Task<PagedResultDto<NameValueDto>> FindUsers(FindUsersInput input)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (tenantId != null)
            {
                //Prevent tenants to get other tenant's users.
                input.TenantId = tenantId;
            }

            int? leaseHaulerIdFilter = null;
            int? customerIdFilter = null;

            if (await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Misc_SelectLists_Users))
            {
                //can see all users
            }
            else if (await PermissionChecker.IsGrantedAsync(AppPermissions.LeaseHaulerPortal_SelectLists_Users))
            {
                //otherwise, can only see LH specific users
                leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
            }
            else if (await PermissionChecker.IsGrantedAsync(AppPermissions.CustomerPortal_SelectLists_Users))
            {
                //otherwise, can only see customer specific users
                customerIdFilter = Session.GetCustomerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException("No permission to see users!");
            }

            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var userIdsWithLeaseHaulerRequestPermission = new List<long>();

                if (leaseHaulerIdFilter.HasValue)
                {
                    userIdsWithLeaseHaulerRequestPermission = await (await (await UserManager.GetQueryAsync())
                            .GetUsersWithGrantedPermission(_roleManager, AppPermissions.Pages_LeaseHaulerRequests))
                        .Select(x => x.Id)
                        .ToListAsync();
                }

                var userIdsVisibleToCustomers = new List<long>();

                if (customerIdFilter.HasValue)
                {
                    userIdsVisibleToCustomers = await (await (await UserManager.GetQueryAsync())
                            .GetUsersWithGrantedPermission(_roleManager, AppPermissions.VisibleToCustomersInChat))
                        .Select(x => x.Id)
                        .ToListAsync();
                }

                var query = (await UserManager.GetQueryAsync())
                    .WhereIf(
                        !input.Filter.IsNullOrWhiteSpace(),
                        u =>
                            u.Name.Contains(input.Filter)
                            || u.Surname.Contains(input.Filter)
                            || u.UserName.Contains(input.Filter)
                            || u.EmailAddress.Contains(input.Filter)
                    )
                    .WhereIf(input.ExcludeCurrentUser, u => u.Id != AbpSession.GetUserId())
                    .WhereIf(leaseHaulerIdFilter.HasValue, u =>
                        u.LeaseHaulerUser.LeaseHaulerId == leaseHaulerIdFilter
                        || userIdsWithLeaseHaulerRequestPermission.Contains(u.Id)
                    )
                    .WhereIf(customerIdFilter.HasValue, u =>
                        u.CustomerContact.CustomerId == customerIdFilter
                        || userIdsVisibleToCustomers.Contains(u.Id)
                    );

                var userCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.Name)
                    .ThenBy(u => u.Surname)
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<NameValueDto>(
                    userCount,
                    users.Select(u =>
                        new NameValueDto(
                            u.FullName + " (" + u.EmailAddress + ")",
                            u.Id.ToString()
                            )
                        ).ToList()
                    );
            }
        }

        public async Task<GetDefaultEditionNameOutput> GetDefaultEditionName()
        {
            await Task.CompletedTask;
            return new GetDefaultEditionNameOutput
            {
                Name = EditionManager.DefaultEditionName,
            };
        }
    }
}
