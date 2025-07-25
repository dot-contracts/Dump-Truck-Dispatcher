using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.BackgroundJobs
{
    public class CacheInvalidationPeriodicJob : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ICacheInvalidationService _cacheInvalidationService;

        public CacheInvalidationPeriodicJob(
            AbpAsyncTimer timer,
            IAppConfigurationAccessor configurationAccessor,
            ICacheInvalidationService cacheInvalidationService
        )
            : base(timer)
        {
            _cacheInvalidationService = cacheInvalidationService;

            var interval = configurationAccessor.Configuration.ParseInt("CacheInvalidation:PeriodicCheckInterval");

            if (interval > 0)
            {
                Timer.Period = (int)TimeSpan.FromSeconds(interval).TotalMilliseconds;
                Timer.RunOnStart = false;
            }
        }

        protected override async Task DoWorkAsync()
        {
            await _cacheInvalidationService.ReceiveNewPersistentInstructions();
        }
    }
}
