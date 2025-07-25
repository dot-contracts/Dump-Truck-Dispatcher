using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Dto;
using DispatcherWeb.HostEmails.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.HostEmails
{
    [AbpAuthorize]
    public class HostEmailAppService : DispatcherWebAppServiceBase, IHostEmailAppService
    {
        private readonly IRepository<HostEmail> _hostEmailRepository;
        private readonly IRepository<HostEmailReceiver> _hostEmailReceiverRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRoleManagementConfig _roleManagementConfig;

        public HostEmailAppService(
            IRepository<HostEmail> hostEmailRepository,
            IRepository<HostEmailReceiver> hostEmailReceiverRepository,
            IBackgroundJobManager backgroundJobManager,
            IRoleManagementConfig roleManagementConfig
            )
        {
            _hostEmailRepository = hostEmailRepository;
            _hostEmailReceiverRepository = hostEmailReceiverRepository;
            _backgroundJobManager = backgroundJobManager;
            _roleManagementConfig = roleManagementConfig;
        }

        [AbpAuthorize(AppPermissions.Pages_HostEmails)]
        public async Task<PagedResultDto<HostEmailDto>> GetHostEmails(GetHostEmailsInput input)
        {
            var timeZone = await GetTimezone();
            var utcDateTimeBegin = input.DateBegin?.ConvertTimeZoneFrom(timeZone);
            var utcDateTimeEnd = input.DateEnd?.AddDays(1).ConvertTimeZoneFrom(timeZone);

            var query = (await _hostEmailRepository.GetQueryAsync())
                .WhereIf(utcDateTimeBegin.HasValue, x => x.CreationTime >= utcDateTimeBegin.Value)
                .WhereIf(utcDateTimeEnd.HasValue, x => x.CreationTime < utcDateTimeEnd.Value)
                .WhereIf(input.EditionId.HasValue, x => x.Receivers.Any(r => r.Tenant.EditionId == input.EditionId))
                .WhereIf(input.TenantId.HasValue, x => x.Receivers.Any(r => r.TenantId == input.TenantId))
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                .WhereIf(input.SentByUserId.HasValue, x => x.CreatorUserId == input.SentByUserId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new HostEmailDto
                {
                    Id = x.Id,
                    SentAtDateTime = x.CreationTime,
                    ProcessedAtDateTime = x.ProcessedAtDateTime,
                    SentByUserFullName = x.CreatorUser.Name + " " + x.CreatorUser.Surname,
                    Type = x.Type,
                    Subject = x.Subject == null ? null : x.Subject.Substring(0, HostEmailDto.MaxLengthOfBodyAndSubject),
                    Body = x.Body == null ? null : x.Body.Substring(0, HostEmailDto.MaxLengthOfBodyAndSubject),
                    Receivers = x.Receivers.Select(x => new HostEmailDto.Receiver
                    {
                        DeliveryStatus = x.TrackableEmail.CalculatedDeliveryStatus,
                    }).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<HostEmailDto>(totalCount, items);
        }

        [AbpAuthorize(AppPermissions.Pages_HostEmails)]
        public async Task<HostEmailViewDto> GetHostEmailForView(EntityDto input)
        {
            var item = await (await _hostEmailRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new HostEmailViewDto
                {
                    Id = x.Id,
                    Subject = x.Subject,
                    Body = x.Body,
                    Type = x.Type,
                    Editions = x.Editions.Select(x => new SelectListDto
                    {
                        Id = x.EditionId.ToString(),
                        Name = x.Edition.Name,
                    }).ToList(),
                    ActiveFilter = x.ActiveFilter,
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

            var staticRoleNames = _roleManagementConfig.StaticRoles
                .Where(r => r.Side == MultiTenancySides.Tenant)
                .Select(r => new SelectListDto
                {
                    Id = r.RoleName,
                    Name = r.RoleDisplayName,
                })
                .ToList();

            foreach (var role in item.Roles)
            {
                var staticRoleName = staticRoleNames.FirstOrDefault(x => x.Id == role.Id);
                if (staticRoleName != null)
                {
                    role.Name = staticRoleName.Name;
                }
            }

            return item;
        }

        [AbpAuthorize(AppPermissions.Pages_HostEmails)]
        public async Task<PagedResultDto<HostEmailReceiverDto>> GetHostEmailReceivers(GetHostEmailReceiversInput input)
        {
            var query = (await _hostEmailReceiverRepository.GetQueryAsync())
                .Where(x => x.HostEmailId == input.HostEmailId);

            var totalCount = await query.CountAsync();

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var items = await query
                    .Select(x => new HostEmailReceiverDto
                    {
                        Id = x.Id,
                        HostEmailProcessedAtDateTime = x.HostEmail.ProcessedAtDateTime,
                        TenantName = x.Tenant.Name,
                        UserFullName = x.User.Name + " " + x.User.Surname,
                        EmailAddress = x.User.EmailAddress,
                        EmailDeliveryStatus = x.TrackableEmail.CalculatedDeliveryStatus,
                    })
                    .OrderBy(input.Sorting)
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<HostEmailReceiverDto>(totalCount, items);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_HostEmails_Send)]
        public async Task SendHostEmail(SendHostEmailInput input)
        {
            var hostEmail = new HostEmail
            {
                Subject = input.Subject,
                Body = input.Body,
                Type = input.Type,
                ActiveFilter = input.ActiveFilter,
                Editions = input.EditionIds.Select(editionId => new HostEmailEdition
                {
                    EditionId = editionId,
                }).ToHashSet(),
                Roles = input.RoleNames.Select(roleName => new HostEmailRole
                {
                    RoleName = roleName,
                }).ToHashSet(),
                Tenants = input.TenantIds.Select(tenantId => new HostEmailTenant
                {
                    TenantId = tenantId,
                }).ToHashSet(),
            };
            await _hostEmailRepository.InsertAndGetIdAsync(hostEmail);

            await _backgroundJobManager.EnqueueAsync<HostEmailSenderBackgroundJob, HostEmailSenderBackgroundJobArgs>(new HostEmailSenderBackgroundJobArgs
            {
                Input = input,
                HostEmailId = hostEmail.Id,
                RequestorUser = await Session.ToUserIdentifierAsync(),
            });
        }
    }
}
