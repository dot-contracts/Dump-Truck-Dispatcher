using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class AppNotifierPriorityNotificationSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<AppNotifierPriorityNotificationSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;

        public AppNotifierPriorityNotificationSenderBackgroundJob(
            IAppNotifier appNotifier,
            IExtendedAbpSession session
        ) : base(session)
        {
            _appNotifier = appNotifier;
        }

        public override async Task ExecuteAsync(AppNotifierPriorityNotificationSenderBackgroundJobArgs args)
        {
            await WithUnitOfWorkAsync(args.RequestorUser, async () =>
            {
                await _appNotifier.SendPriorityNotificationImmediately(args.SendPriorityNotificationInput);
            });
        }
    }
}
