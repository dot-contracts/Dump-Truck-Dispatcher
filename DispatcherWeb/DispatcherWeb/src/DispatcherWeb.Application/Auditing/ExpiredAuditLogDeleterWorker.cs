using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.EFPlus;
using Abp.Logging;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Auditing
{
    public class ExpiredAuditLogDeleterWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        /// <summary>
        /// Set this const field to true if you want to enable ExpiredAuditLogDeleterWorker.
        /// Be careful, If you enable this, all expired logs will be permanently deleted.
        /// </summary>
        public const bool IsEnabled = false;

        private const int CheckPeriodAsMilliseconds = 1 * 1000 * 60 * 3; // 3min
        private const int MaxDeletionCount = 10000;

        private readonly TimeSpan _logExpireTime = TimeSpan.FromDays(7);
        private readonly IRepository<AuditLog, long> _auditLogRepository;
        private readonly IRepository<Tenant> _tenantRepository;

        public ExpiredAuditLogDeleterWorker(
            AbpAsyncTimer timer,
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<Tenant> tenantRepository
            )
            : base(timer)
        {
            _auditLogRepository = auditLogRepository;
            _tenantRepository = tenantRepository;

            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;

            Timer.Period = CheckPeriodAsMilliseconds;
            Timer.RunOnStart = false;
        }

        protected override async Task DoWorkAsync()
        {
            var expireDate = Clock.Now - _logExpireTime;

            var tenantIds = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                return await (await _tenantRepository.GetQueryAsync())
                    .Where(t => !string.IsNullOrEmpty(t.ConnectionString))
                    .Select(t => t.Id)
                    .ToListAsync();
            });

            await DeleteAuditLogsOnHostDatabaseAsync(expireDate);

            foreach (var tenantId in tenantIds)
            {
                await DeleteAuditLogsOnTenantDatabaseAsync(tenantId, expireDate);
            }
        }

        protected virtual async Task DeleteAuditLogsOnHostDatabaseAsync(DateTime expireDate)
        {
            try
            {
                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(null))
                    {
                        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                        {
                            await DeleteAuditLogsAsync(expireDate);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Log(LogSeverity.Error, $"An error occured while deleting audit logs on host database", e);
            }
        }

        protected virtual async Task DeleteAuditLogsOnTenantDatabaseAsync(int tenantId, DateTime expireDate)
        {
            try
            {
                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(tenantId))
                    {
                        await DeleteAuditLogsAsync(expireDate);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Log(LogSeverity.Error, $"An error occured while deleting audit log for tenant. TenantId: {tenantId}", e);
            }
        }

        private async Task DeleteAuditLogsAsync(DateTime expireDate)
        {
            var expiredEntryCount = await _auditLogRepository.LongCountAsync(l => l.ExecutionTime < expireDate);

            if (expiredEntryCount == 0)
            {
                return;
            }

            if (expiredEntryCount > MaxDeletionCount)
            {
                var deleteStartId = await (await _auditLogRepository.GetQueryAsync()).OrderBy(l => l.Id).Skip(MaxDeletionCount).Select(x => x.Id).FirstAsync();

                await _auditLogRepository.BatchDeleteAsync(l => l.Id < deleteStartId);
            }
            else
            {
                await _auditLogRepository.BatchDeleteAsync(l => l.ExecutionTime < expireDate);
            }
        }
    }
}
