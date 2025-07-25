using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class AppNotifierNotificationSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<AppNotifierNotificationSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;

        public AppNotifierNotificationSenderBackgroundJob(
            IAppNotifier appNotifier,
            IExtendedAbpSession session
        ) : base(session)
        {
            _appNotifier = appNotifier;
        }

        public override async Task ExecuteAsync(AppNotifierNotificationSenderBackgroundJobArgs args)
        {
            await WithUnitOfWorkAsync(args.RequestorUser, async () =>
            {
                await _appNotifier.SendNotificationImmediatelyAsync(args.SendNotificationInput);
            });
        }
    }
}
