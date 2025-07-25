using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityHistory;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.Auditing.Exporting;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Dto;
using DispatcherWeb.EntityHistory;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using EntityHistoryHelper = DispatcherWeb.EntityHistory.EntityHistoryHelper;

namespace DispatcherWeb.Auditing
{
    [DisableAuditing]
    [AbpAuthorize(AppPermissions.Pages_Administration_AuditLogs)]
    public class AuditLogAppService : DispatcherWebAppServiceBase, IAuditLogAppService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRepository<EntityChange, long> _entityChangeRepository;
        private readonly IRepository<EntityChangeSet, long> _entityChangeSetRepository;
        private readonly IRepository<EntityPropertyChange, long> _entityPropertyChangeRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IAuditLogListExcelExporter _auditLogListExcelExporter;
        private readonly INamespaceStripper _namespaceStripper;
        private readonly IAbpStartupConfiguration _abpStartupConfiguration;

        public AuditLogAppService(
            IAuditLogRepository auditLogRepository,
            IRepository<User, long> userRepository,
            IRepository<Tenant> tenantRepository,
            IAuditLogListExcelExporter auditLogListExcelExporter,
            INamespaceStripper namespaceStripper,
            IRepository<EntityChange, long> entityChangeRepository,
            IRepository<EntityChangeSet, long> entityChangeSetRepository,
            IRepository<EntityPropertyChange, long> entityPropertyChangeRepository,
            IAbpStartupConfiguration abpStartupConfiguration)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _auditLogListExcelExporter = auditLogListExcelExporter;
            _namespaceStripper = namespaceStripper;
            _entityChangeRepository = entityChangeRepository;
            _entityChangeSetRepository = entityChangeSetRepository;
            _entityPropertyChangeRepository = entityPropertyChangeRepository;
            _abpStartupConfiguration = abpStartupConfiguration;
        }

        #region audit logs

        public async Task<PagedResultDto<AuditLogListDto>> GetAuditLogs(GetAuditLogsInput input)
        {
            if (input.ShowForAllTenants)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Administration_AuditLogs_ViewAllTenants);
                CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant);
            }

            CurrentUnitOfWork.Options.Timeout = TimeSpan.FromMinutes(10);
            var query = await CreateAuditLogAndUsersQueryAsync(input);

            var resultCount = await query.CountAsync();
            var results = await query
                .AsNoTracking()
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            if (input.ShowForAllTenants)
            {
                CurrentUnitOfWork.EnableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant);
            }

            var auditLogListDtos = ConvertToAuditLogListDtos(results);

            return new PagedResultDto<AuditLogListDto>(resultCount, auditLogListDtos);
        }

        public async Task<FileDto> GetAuditLogsToExcel(GetAuditLogsInput input)
        {
            if (input.ShowForAllTenants)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Administration_AuditLogs_ViewAllTenants);
                CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant);
            }

            var query = await CreateAuditLogAndUsersQueryAsync(input);

            var auditLogs = await query
                .AsNoTracking()
                .OrderByDescending(al => al.ExecutionTime)
                .ToListAsync();

            if (input.ShowForAllTenants)
            {
                CurrentUnitOfWork.EnableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant);
            }

            if (auditLogs.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            var auditLogListDtos = ConvertToAuditLogListDtos(auditLogs);

            return await _auditLogListExcelExporter.ExportToFileAsync(auditLogListDtos, input.ShowForAllTenants);
        }

        private List<AuditLogListDto> ConvertToAuditLogListDtos(List<AuditLogListDto> results)
        {
            foreach (var item in results)
            {
                item.ServiceName = _namespaceStripper.StripNameSpace(item.ServiceName);
            }

            return results;
        }

        private async Task<IQueryable<AuditLogListDto>> CreateAuditLogAndUsersQueryAsync(GetAuditLogsInput input)
        {
            var query = from auditLog in await _auditLogRepository.GetQueryAsync()
                        join tenant in await _tenantRepository.GetQueryAsync() on auditLog.TenantId equals tenant.Id into tenantJoin
                        from joinedTenant in tenantJoin.DefaultIfEmpty()
                        join user in await _userRepository.GetQueryAsync() on auditLog.UserId equals user.Id into userJoin
                        from joinedUser in userJoin.DefaultIfEmpty()
                        where auditLog.ExecutionTime >= input.StartDate && auditLog.ExecutionTime <= input.EndDate
                        select new AuditLogAndUser { AuditLog = auditLog, User = joinedUser, Tenant = joinedTenant };

            query = query
                .WhereIf(!input.UserName.IsNullOrWhiteSpace(), item => item.User.UserName.Contains(input.UserName))
                .WhereIf(!input.ServiceName.IsNullOrWhiteSpace(), item => item.AuditLog.ServiceName.Contains(input.ServiceName))
                .WhereIf(!input.MethodName.IsNullOrWhiteSpace(), item => item.AuditLog.MethodName.Contains(input.MethodName))
                .WhereIf(!input.BrowserInfo.IsNullOrWhiteSpace(), item => item.AuditLog.BrowserInfo.Contains(input.BrowserInfo))
                .WhereIf(input.MinExecutionDuration > 0, item => item.AuditLog.ExecutionDuration >= input.MinExecutionDuration.Value)
                .WhereIf(input.MaxExecutionDuration < int.MaxValue, item => item.AuditLog.ExecutionDuration <= input.MaxExecutionDuration.Value)
                .WhereIf(input.ErrorState == AuditLogErrorState.HasError, item => item.AuditLog.Exception != null && item.AuditLog.Exception != "")
                .WhereIf(input.ErrorState == AuditLogErrorState.Success, item => item.AuditLog.Exception == null || item.AuditLog.Exception == "")
                .WhereIf(input.ErrorState == AuditLogErrorState.MeaningfulErrors, item => item.AuditLog.Exception != null && item.AuditLog.MethodName.ToLower() != "login"
                    && !item.AuditLog.Exception.StartsWith("Abp.UI.UserFriendlyException:")
                    && !item.AuditLog.Exception.StartsWith("Abp.Runtime.Validation.AbpValidationException:"));

            return query.Select(x => new AuditLogListDto
            {
                Id = x.AuditLog.Id,
                ExecutionTime = x.AuditLog.ExecutionTime,
                ExecutionDuration = x.AuditLog.ExecutionDuration,
                ServiceName = x.AuditLog.ServiceName,
                MethodName = x.AuditLog.MethodName,
                Parameters = x.AuditLog.Parameters,
                BrowserInfo = x.AuditLog.BrowserInfo,
                ClientIpAddress = x.AuditLog.ClientIpAddress,
                ClientName = x.AuditLog.ClientName,
                UserId = x.AuditLog.UserId,
                UserName = x.User.Name,
                ImpersonatorTenantId = x.AuditLog.ImpersonatorTenantId,
                ImpersonatorUserId = x.AuditLog.ImpersonatorUserId,
                Exception = x.AuditLog.Exception,
                CustomData = x.AuditLog.CustomData,
                TenantName = x.Tenant.Name ?? "",
            });
        }

        #endregion

        #region entity changes
        public async Task<List<NameValueDto>> GetEntityHistoryObjectTypesAsync()
        {
            var entityHistoryObjectTypes = new List<NameValueDto>();
            var enabledEntities = (_abpStartupConfiguration.GetCustomConfig()
                .FirstOrDefault(x => x.Key == EntityHistoryHelper.EntityHistoryConfigurationName)
                .Value as EntityHistoryUiSetting)?.EnabledEntities ?? new List<string>();

            if (await AbpSession.GetTenantIdOrNullAsync() == null)
            {
                enabledEntities = EntityHistoryHelper.HostSideTrackedTypes.Select(t => t.FullName).Intersect(enabledEntities).ToList();
            }
            else
            {
                enabledEntities = EntityHistoryHelper.TenantSideTrackedTypes.Select(t => t.FullName).Intersect(enabledEntities).ToList();
            }

            foreach (var enabledEntity in enabledEntities)
            {
                entityHistoryObjectTypes.Add(new NameValueDto(L(enabledEntity), enabledEntity));
            }

            return entityHistoryObjectTypes;
        }

        public async Task<PagedResultDto<EntityChangeListDto>> GetEntityChanges(GetEntityChangeInput input)
        {
            var query = await CreateEntityChangesAndUsersQueryAsync(input);

            var resultCount = await query.CountAsync();
            var results = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EntityChangeListDto>(resultCount, results);
        }

        public async Task<PagedResultDto<EntityChangeListDto>> GetEntityTypeChanges(GetEntityChangeInput input)
        {
            var query = await CreateEntityChangesAndUsersQueryAsync(input);

            var resultCount = await query.CountAsync();
            var results = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EntityChangeListDto>(resultCount, results);
        }

        public async Task<FileDto> GetEntityChangesToExcel(GetEntityChangeInput input)
        {
            var entityChanges = await (await CreateEntityChangesAndUsersQueryAsync(input))
                .AsNoTracking()
                .OrderByDescending(ec => ec.EntityChangeSetId)
                .ThenByDescending(ec => ec.ChangeTime)
                .ToListAsync();

            if (entityChanges.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            return await _auditLogListExcelExporter.ExportToFileAsync(entityChanges);
        }

        public async Task<List<EntityPropertyChangeDto>> GetEntityPropertyChanges(long entityChangeId)
        {
            var entityPropertyChanges = await (await _entityPropertyChangeRepository.GetQueryAsync())
                .Where(epc => epc.EntityChangeId == entityChangeId)
                .Select(x => new EntityPropertyChangeDto
                {
                    Id = x.Id,
                    EntityChangeId = entityChangeId,
                    NewValue = x.NewValue,
                    OriginalValue = x.OriginalValue,
                    PropertyName = x.PropertyName,
                    PropertyTypeFullName = x.PropertyTypeFullName,
                    TenantId = x.TenantId,
                }).ToListAsync();

            return entityPropertyChanges;
        }

        private async Task<IQueryable<EntityChangeListDto>> CreateEntityChangesAndUsersQueryAsync(GetEntityChangeInput input)
        {
            input.ValidateInput();

            var query = from entityChangeSet in await _entityChangeSetRepository.GetQueryAsync()
                        join entityChange in await _entityChangeRepository.GetQueryAsync() on entityChangeSet.Id equals entityChange.EntityChangeSetId
                        join user in await _userRepository.GetQueryAsync() on entityChangeSet.UserId equals user.Id
                        select new EntityChangeAndUser
                        {
                            EntityChange = entityChange,
                            User = user,
                        };

            query = query
                .WhereIf(input.StartDate.HasValue, item => item.EntityChange.ChangeTime >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, item => item.EntityChange.ChangeTime <= input.EndDate)
                .WhereIf(!input.UserName.IsNullOrWhiteSpace(), item => item.User.UserName.Contains(input.UserName));

            //we need exact match by EntityTypeFullName if entity id is specified and Contains match by EntityTypeFullName otherwise
            if (!input.EntityId.IsNullOrWhiteSpace() && !input.EntityTypeFullName.IsNullOrWhiteSpace())
            {
                // Fix for: https://github.com/aspnetzero/aspnet-zero-core/issues/2101
                var escapedEntityId = "\"" + input.EntityId + "\"";

                query = query
                    .Where(x => x.EntityChange.EntityTypeFullName == input.EntityTypeFullName
                                && (x.EntityChange.EntityId == input.EntityId || x.EntityChange.EntityId == escapedEntityId));
            }
            else
            {
                query = query
                    .WhereIf(!input.EntityTypeFullName.IsNullOrWhiteSpace(), item => item.EntityChange.EntityTypeFullName.Contains(input.EntityTypeFullName));
            }

            return query.Select(x => new EntityChangeListDto
            {
                Id = x.EntityChange.Id,
                UserId = x.User.Id,
                ChangeTime = x.EntityChange.ChangeTime,
                EntityTypeFullName = x.EntityChange.EntityTypeFullName,
                ChangeType = x.EntityChange.ChangeType,
                EntityChangeSetId = x.EntityChange.EntityChangeSetId,
                UserName = x.User.UserName,
            });
        }
        #endregion

        public string Is64BitProcess()
        {
            return Environment.Is64BitProcess.ToString();
        }

        [UnitOfWork(IsDisabled = true)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task RemoveOldAuditLogsAsync()
        {
            try
            {
                Logger.Info($"RemoveOldAuditLogs started at {Clock.Now:s}");

                await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
                {
                    IsTransactional = false,
                    Timeout = TimeSpan.FromMinutes(60),
                }, async () =>
                {
                    await _auditLogRepository.DeleteOldAuditLogsAsync();
                });
                Logger.Info($"RemoveOldAuditLogs finished at {Clock.Now:s}");
            }
            catch (Exception e)
            {
                Logger.Error("RemoveOldAuditLogs failed", e);
                throw;
            }
        }
    }
}
