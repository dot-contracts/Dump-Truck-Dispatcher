using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.MultiTenancy
{
    public class SubscriptionExpireEmailNotifierWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private const int CheckPeriodAsMilliseconds = 1 * 60 * 60 * 1000 * 24; //1 day

        private readonly IRepository<Tenant> _tenantRepository;
        private readonly UserEmailer _userEmailer;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public SubscriptionExpireEmailNotifierWorker(
            AbpAsyncTimer timer,
            IRepository<Tenant> tenantRepository,
            UserEmailer userEmailer,
            IUnitOfWorkManager unitOfWorkManager) : base(timer)
        {
            _tenantRepository = tenantRepository;
            _userEmailer = userEmailer;
            _unitOfWorkManager = unitOfWorkManager;

            Timer.Period = CheckPeriodAsMilliseconds;
            Timer.RunOnStart = false;

            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected override async Task DoWorkAsync()
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var subscriptionRemainingDayCount = Convert.ToInt32(await SettingManager.GetSettingValueForApplicationAsync(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount));
                var dateToCheckRemainingDayCount = Clock.Now.AddDays(subscriptionRemainingDayCount).ToUniversalTime();

                var subscriptionExpiredTenants = await _tenantRepository.GetAllListAsync(
                    tenant => tenant.SubscriptionEndDateUtc != null
                              && tenant.SubscriptionEndDateUtc.Value.Date == dateToCheckRemainingDayCount.Date
                              && tenant.IsActive
                              && tenant.EditionId != null
                );

                foreach (var tenant in subscriptionExpiredTenants)
                {
                    Debug.Assert(tenant.EditionId.HasValue);
                    try
                    {
                        await _userEmailer.TryToSendSubscriptionExpiringSoonEmail(tenant.Id, dateToCheckRemainingDayCount);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.Message, exception);
                    }
                }
            });
        }
    }
}
