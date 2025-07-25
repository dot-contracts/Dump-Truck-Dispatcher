using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.BackgroundJobs
{
    public class RemoveStaleOnlineClientsPeriodicJob : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly int _onlineClientTimeout;
        private readonly IAsyncOnlineClientManager _onlineClientManager;

        public RemoveStaleOnlineClientsPeriodicJob(
            AbpAsyncTimer timer,
            IAppConfigurationAccessor configurationAccessor,
            IAsyncOnlineClientManager onlineClientManager
            )
            : base(timer)
        {
            _onlineClientManager = onlineClientManager;

            var period = configurationAccessor.Configuration.ParseInt("SignalR:OnlineClientTimeout");

            if (period > 0)
            {
                _onlineClientTimeout = period;

                Timer.Period = (int)TimeSpan.FromSeconds(period).TotalMilliseconds;
                Timer.RunOnStart = true;
            }
        }

        protected override async Task DoWorkAsync()
        {
            var cutoffDate = Clock.Now.Subtract(TimeSpan.FromSeconds(_onlineClientTimeout));
            await _onlineClientManager.RemoveAllOlderThan(cutoffDate);
        }
    }
}
