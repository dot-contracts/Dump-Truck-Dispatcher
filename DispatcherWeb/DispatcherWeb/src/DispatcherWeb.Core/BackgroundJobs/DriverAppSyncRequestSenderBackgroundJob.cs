using System;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverAppSyncRequestSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<DriverAppSyncRequestSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IDriverAppSyncRequestSender _driverAppSyncRequestSender;

        public DriverAppSyncRequestSenderBackgroundJob(
            IExtendedAbpSession session,
            IDriverAppSyncRequestSender driverAppSyncRequestSender
            ) : base(session)
        {
            _driverAppSyncRequestSender = driverAppSyncRequestSender;
        }

        public override async Task ExecuteAsync(DriverAppSyncRequestSenderBackgroundJobArgs args)
        {
            try
            {
                await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                {
                    await _driverAppSyncRequestSender.SendSyncRequestAsync(args.GetSyncRequest());
                });
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected error in DriverAppSyncRequestSenderBackgroundJob: " + e.Message, e);
            }
        }
    }
}
