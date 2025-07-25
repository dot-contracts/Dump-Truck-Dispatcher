using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.CustomerNotifications.Cache;
using DispatcherWeb.CustomerNotifications.Dto;
using DispatcherWeb.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CustomerNotifications
{
    [AbpAuthorize]
    public class CustomerNotificationAppService : DispatcherWebAppServiceBase, ICustomerNotificationAppService
    {
        private readonly IRepository<CustomerNotification> _customerNotificationRepository;
        private readonly IRepository<DismissedCustomerNotification> _dismissedCustomerNotificationRepository;
        private readonly IRepository<CustomerNotificationEdition> _customerNotificationEditionRepository;
        private readonly IRepository<CustomerNotificationTenant> _customerNotificationTenantRepository;
        private readonly IRepository<CustomerNotificationRole> _customerNotificationRoleRepository;
        private readonly ICustomerNotificationCache _customerNotificationCache;
        private readonly ITenantCache _tenantCache;

        public CustomerNotificationAppService(
            IRepository<CustomerNotification> customerNotificationRepository,
            IRepository<DismissedCustomerNotification> dismissedCustomerNotificationRepository,
            IRepository<CustomerNotificationEdition> customerNotificationEditionRepository,
            IRepository<CustomerNotificationTenant> customerNotificationTenantRepository,
            IRepository<CustomerNotificationRole> customerNotificationRoleRepository,
            ICustomerNotificationCache customerNotificationCache,
            ITenantCache tenantCache)
        {
            _customerNotificationRepository = customerNotificationRepository;
            _dismissedCustomerNotificationRepository = dismissedCustomerNotificationRepository;
            _customerNotificationEditionRepository = customerNotificationEditionRepository;
            _customerNotificationTenantRepository = customerNotificationTenantRepository;
            _customerNotificationRoleRepository = customerNotificationRoleRepository;
            _customerNotificationCache = customerNotificationCache;
            _tenantCache = tenantCache;
        }

        [AbpAuthorize(AppPermissions.Pages_CustomerNotifications)]
        public async Task<PagedResultDto<CustomerNotificationDto>> GetCustomerNotifications(GetCustomerNotificationsInput input)
        {
            var query = (await _customerNotificationRepository.GetQueryAsync())
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                .WhereIf(input.EditionId.HasValue, x => x.Editions.Any(e => e.EditionId == input.EditionId))
                .WhereIf(input.TenantId.HasValue, x => x.Tenants.Any(e => e.TenantId == input.TenantId))
                .WhereIf(input.CreatedByUserId.HasValue, x => x.CreatorUserId == input.CreatedByUserId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new CustomerNotificationDto
                {
                    Id = x.Id,
                    CreatedByUserFullName = x.CreatorUser.Name + " " + x.CreatorUser.Surname,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Title = x.Title == null ? null : x.Title.Substring(0, CustomerNotificationDto.MaxLengthOfBodyAndTitle),
                    Body = x.Body == null ? null : x.Body.Substring(0, CustomerNotificationDto.MaxLengthOfBodyAndTitle),
                    EditionNames = x.Editions.Select(e => e.Edition.Name).ToList(),
                    Type = x.Type,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<CustomerNotificationDto>(totalCount, items);
        }

        [AbpAuthorize(AppPermissions.Pages_CustomerNotifications)]
        public async Task<CustomerNotificationEditDto> GetCustomerNotificationForEdit(NullableIdDto input)
        {
            CustomerNotificationEditDto customerNotification;

            if (input.Id.HasValue)
            {
                customerNotification = await (await _customerNotificationRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new CustomerNotificationEditDto
                    {
                        Id = x.Id,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Title = x.Title,
                        Body = x.Body,
                        Type = x.Type,
                        Editions = x.Editions.Select(x => new SelectListDto
                        {
                            Id = x.EditionId.ToString(),
                            Name = x.Edition.Name,
                        }).ToList(),
                        Tenants = x.Tenants.Select(x => new SelectListDto
                        {
                            Id = x.TenantId.ToString(),
                            Name = x.Tenant.Name,
                        }).ToList(),
                        Roles = x.Roles.Select(x => new SelectListDto
                        {
                            Id = x.RoleName.ToString(),
                            Name = x.RoleName.ToString(),
                        }).ToList(),
                    }).FirstAsync();
            }
            else
            {
                customerNotification = new CustomerNotificationEditDto();
            }

            return customerNotification;
        }

        [AbpAuthorize(AppPermissions.Pages_CustomerNotifications_Edit)]
        public async Task EditCustomerNotification(CustomerNotificationEditDto input)
        {
            var entity = input.Id.HasValue ? await _customerNotificationRepository.GetAsync(input.Id.Value) : new CustomerNotification();

            entity.StartDate = input.StartDate.Value;
            entity.EndDate = input.EndDate.Value;
            entity.Title = input.Title;
            entity.Body = input.Body;
            entity.Type = input.Type;

            input.Id = await _customerNotificationRepository.InsertOrUpdateAndGetIdAsync(entity);

            var existingEditions = await (await _customerNotificationEditionRepository.GetQueryAsync())
                .Where(e => e.CustomerNotificationId == input.Id)
                .ToListAsync();

            var editionsToDelete = existingEditions
                .Where(e => !input.EditionIds.Any(i => i == e.EditionId))
                .ToList();
            await _customerNotificationEditionRepository.DeleteRangeAsync(editionsToDelete);

            var editionsToAdd = input.EditionIds
                .Where(x => !existingEditions.Any(e => e.EditionId == x))
                .Select(i => new CustomerNotificationEdition
                {
                    CustomerNotificationId = entity.Id,
                    EditionId = i,
                })
                .ToList();
            await _customerNotificationEditionRepository.InsertRangeAsync(editionsToAdd);


            var existingTenants = await (await _customerNotificationTenantRepository.GetQueryAsync())
                .Where(e => e.CustomerNotificationId == input.Id)
                .ToListAsync();
            var tenantsToDelete = existingTenants
                .Where(e => !input.TenantIds.Any(i => i == e.TenantId))
                .ToList();
            await _customerNotificationTenantRepository.DeleteRangeAsync(tenantsToDelete);

            var tenantsToAdd = input.TenantIds
                .Where(x => !existingTenants.Any(e => e.TenantId == x))
                .Select(i => new CustomerNotificationTenant
                {
                    CustomerNotificationId = entity.Id,
                    TenantId = i,
                })
                .ToList();
            await _customerNotificationTenantRepository.InsertRangeAsync(tenantsToAdd);


            var existingRoles = await (await _customerNotificationRoleRepository.GetQueryAsync())
                .Where(e => e.CustomerNotificationId == input.Id)
                .ToListAsync();
            var rolesToDelete = existingRoles
                .Where(e => !input.RoleNames.Any(i => i == e.RoleName))
                .ToList();
            await _customerNotificationRoleRepository.DeleteRangeAsync(rolesToDelete);

            var rolesToAdd = input.RoleNames
                .Where(x => !existingRoles.Any(e => e.RoleName == x))
                .Select(i => new CustomerNotificationRole
                {
                    CustomerNotificationId = entity.Id,
                    RoleName = i,
                })
                .ToList();
            await _customerNotificationRoleRepository.InsertRangeAsync(rolesToAdd);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _customerNotificationCache.InvalidateCache();
        }

        [AbpAuthorize(AppPermissions.Pages_CustomerNotifications_Edit)]
        public async Task DeleteCustomerNotification(EntityDto input)
        {
            await _customerNotificationRepository.DeleteAsync(input.Id);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _customerNotificationCache.InvalidateCache();
        }

        public async Task<List<CustomerNotificationToShowDto>> GetCustomerNotificationsToShow()
        {
            var tenantId = await Session.GetTenantIdOrNullAsync();
            if (tenantId == null)
            {
                return new();
            }

            var today = await GetToday();
            var cachedResponse = await _customerNotificationCache.GetFromCacheOrDefault(today, Session.GetUserId());
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var editionId = (await _tenantCache.GetAsync(tenantId.Value)).EditionId;

            var user = await GetCurrentUserAsync();
            var roleNames = await UserManager.GetRolesAsync(user);

            var query = (await _customerNotificationRepository.GetQueryAsync())
                .Where(x => x.StartDate <= today && today <= x.EndDate)
                .Where(x => !x.Tenants.Any() || x.Tenants.Any(t => t.TenantId == tenantId))
                .Where(x => !x.Editions.Any() || x.Editions.Any(e => e.EditionId == editionId))
                .Where(x => !x.Roles.Any() || x.Roles.Any(r => roleNames.Contains(r.RoleName)))
                .Where(x => !x.Dismissions.Any(d => d.UserId == Session.UserId));

            var customerNotificationIds = await query
                .Select(x => x.Id)
                .ToListAsync();

            var items = await _customerNotificationCache.StoreAndEnrichUserNotifications(today, Session.GetUserId(), customerNotificationIds);

            return items;
        }

        public async Task DismissCustomerNotifications(CustomerNotificationToDismissInput input)
        {
            await _customerNotificationCache.DismissCustomerNotification(await GetToday(), Session.GetUserId(), input.Id);
            var dismissedCustomerNotification = new DismissedCustomerNotification
            {
                CustomerNotificationId = input.Id,
                UserId = Session.GetUserId(),
            };
            await _dismissedCustomerNotificationRepository.InsertAndGetIdAsync(dismissedCustomerNotification);
        }
    }
}
