using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Editions;

namespace DispatcherWeb.MultiTenancy
{
    public class SubscriptionExpirationCheckWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private const int CheckPeriodAsMilliseconds = 1 * 60 * 60 * 1000; //1 hour

        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<SubscribableEdition> _editionRepository;
        private readonly TenantManager _tenantManager;
        private readonly UserEmailer _userEmailer;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public SubscriptionExpirationCheckWorker(
            AbpAsyncTimer timer,
            IRepository<Tenant> tenantRepository,
            IRepository<SubscribableEdition> editionRepository,
            TenantManager tenantManager,
            UserEmailer userEmailer,
            IUnitOfWorkManager unitOfWorkManager)
            : base(timer)
        {
            _tenantRepository = tenantRepository;
            _editionRepository = editionRepository;
            _tenantManager = tenantManager;
            _userEmailer = userEmailer;

            Timer.Period = CheckPeriodAsMilliseconds;
            Timer.RunOnStart = false;

            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
            _unitOfWorkManager = unitOfWorkManager;
        }

        protected override async Task DoWorkAsync()
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var utcNow = Clock.Now.ToUniversalTime();
                var failedTenancyNames = new List<string>();

                var subscriptionExpiredTenants = await _tenantRepository.GetAllListAsync(
                    tenant => tenant.SubscriptionEndDateUtc != null
                              && tenant.SubscriptionEndDateUtc <= utcNow
                              && tenant.IsActive
                              && tenant.EditionId != null
                );

                foreach (var tenant in subscriptionExpiredTenants)
                {
                    Debug.Assert(tenant.EditionId.HasValue);

                    try
                    {

                        var edition = await _editionRepository.GetAsync(tenant.EditionId.Value);

                        Debug.Assert(tenant.SubscriptionEndDateUtc != null, "tenant.SubscriptionEndDateUtc != null");

                        if (tenant.SubscriptionEndDateUtc.Value.AddDays(edition.WaitingDayAfterExpire ?? 0) >= utcNow)
                        {
                            //Tenant is in waiting days after expire TODO: It's better to filter such entities while querying from repository!
                            continue;
                        }

                        var endSubscriptionResult = await _tenantManager.EndSubscriptionAsync(tenant, edition, utcNow);

                        if (endSubscriptionResult == EndSubscriptionResult.TenantSetInActive)
                        {
                            await _userEmailer.TryToSendSubscriptionExpireEmail(tenant.Id, utcNow);
                        }
                        else if (endSubscriptionResult == EndSubscriptionResult.AssignedToAnotherEdition)
                        {
                            await _userEmailer.TryToSendSubscriptionAssignedToAnotherEmail(tenant.Id, utcNow, edition.ExpiringEditionId.Value);
                        }
                    }
                    catch (Exception exception)
                    {
                        failedTenancyNames.Add(tenant.TenancyName);
                        Logger.Error($"Subscription of tenant {tenant.TenancyName} has been expired but tenant couldn't be made passive !");
                        Logger.Error(exception.Message, exception);
                    }
                }

                if (!failedTenancyNames.Any())
                {
                    return;
                }

                await _userEmailer.TryToSendFailedSubscriptionTerminationsEmail(failedTenancyNames, utcNow);
            });
        }
    }
}
