using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Notifications;
using DispatcherWeb.Fulcrum;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class FulcrumSyncJob : DispatcherWebAsyncBackgroundJobBase<FulcrumSyncJobArgs>, ITransientDependency
    {
        private readonly IFulcrumAppService _fulcrumAppService;
        private readonly INotificationPublisher _notificationPublisher;

        public FulcrumSyncJob(
            IFulcrumAppService fulcrumAppService,
            INotificationPublisher notificationPublisher,
            IExtendedAbpSession session
        ) : base(session)
        {
            _fulcrumAppService = fulcrumAppService;
            _notificationPublisher = notificationPublisher;
        }

        public override async Task ExecuteAsync(FulcrumSyncJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser))
                {
                    await _notificationPublisher.PublishAsync(
                        AppNotificationNames.FulcrumSyncStarted,
                        new MessageNotificationData($"We are synching your {args.Entity.GetDisplayName()} with Fulcrum. This may take some time, we will notify you when it is done."),
                        null,
                        NotificationSeverity.Info,
                        userIds: new[] { args.RequestorUser }
                    );

                    await _fulcrumAppService.SyncFulcrumEntityAsync(args.Entity);

                    await _notificationPublisher.PublishAsync(
                        AppNotificationNames.FulcrumSyncCompleted,
                        new MessageNotificationData($"Fulcrum {args.Entity.GetDisplayName()} sync has been completed."),
                        null,
                        NotificationSeverity.Success,
                        userIds: new[] { args.RequestorUser }
                    );
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error when syncing with fulcrum: {e}");
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.FulcrumSyncError,
                    new MessageNotificationData("Syncing with Fulcrum failed."),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { args.RequestorUser }
                );
            }
        }
    }
}
