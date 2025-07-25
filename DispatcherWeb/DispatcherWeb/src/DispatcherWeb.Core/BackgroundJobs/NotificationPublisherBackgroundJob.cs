using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Notifications;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class NotificationPublisherBackgroundJob : DispatcherWebAsyncBackgroundJobBase<NotificationPublisherBackgroundJobArgs>, ITransientDependency
    {
        private readonly DispatcherWebNotificationPublisher _notificationPublisher;

        public NotificationPublisherBackgroundJob(
            DispatcherWebNotificationPublisher notificationPublisher,
            IExtendedAbpSession session
        ) : base(session)
        {
            _notificationPublisher = notificationPublisher;
        }

        public override async Task ExecuteAsync(NotificationPublisherBackgroundJobArgs args)
        {
            await WithUnitOfWorkAsync(args.RequestorUser, async () =>
            {
                await _notificationPublisher.PublishImmediatelyAsync(
                    notificationName: args.NotificationName,
                    data: Utilities.DeserializeWithTypes<NotificationData>(args.NotificationDataString),
                    entityIdentifier: args.EntityIdentifier,
                    severity: args.Severity,
                    userIds: args.UserIds,
                    excludedUserIds: args.ExcludedUserIds,
                    tenantIds: args.TenantIds
                );
            });
        }
    }
}
