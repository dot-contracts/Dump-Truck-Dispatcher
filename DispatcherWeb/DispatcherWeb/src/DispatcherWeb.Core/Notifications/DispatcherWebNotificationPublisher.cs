using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Domain.Entities;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.BackgroundJobs;

namespace DispatcherWeb.Notifications
{
    public class DispatcherWebNotificationPublisher : NotificationPublisher/*, IExtendedNotificationPublisher, ITransientDependency*/
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAbpSession _session;

        public DispatcherWebNotificationPublisher(
            IAbpSession session,
            INotificationStore store,
            IBackgroundJobManager backgroundJobManager,
            INotificationDistributer notificationDistributer,
            IGuidGenerator guidGenerator
        ) : base(
            store,
            backgroundJobManager,
            notificationDistributer,
            guidGenerator
        )
        {
            _session = session;
            _backgroundJobManager = backgroundJobManager;
        }

        public override async Task PublishAsync(
            string notificationName,
            NotificationData data = null,
            EntityIdentifier entityIdentifier = null,
            NotificationSeverity severity = NotificationSeverity.Info,
            UserIdentifier[] userIds = null,
            UserIdentifier[] excludedUserIds = null,
            int?[] tenantIds = null
        )
        {
            await _backgroundJobManager.EnqueueAsync<NotificationPublisherBackgroundJob, NotificationPublisherBackgroundJobArgs>(new NotificationPublisherBackgroundJobArgs
            {
                RequestorUser = await _session.ToUserIdentifierAsync(),
                NotificationName = notificationName,
                NotificationDataString = Utilities.SerializeWithTypes(data),
                EntityIdentifier = entityIdentifier,
                Severity = severity,
                UserIds = userIds,
                ExcludedUserIds = excludedUserIds,
                TenantIds = tenantIds,
            });
        }

        public Task PublishImmediatelyAsync(
            string notificationName,
            NotificationData data = null,
            EntityIdentifier entityIdentifier = null,
            NotificationSeverity severity = NotificationSeverity.Info,
            UserIdentifier[] userIds = null,
            UserIdentifier[] excludedUserIds = null,
            int?[] tenantIds = null
        )
        {
            return base.PublishAsync(
                notificationName,
                data,
                entityIdentifier,
                severity,
                userIds,
                excludedUserIds,
                tenantIds
            );
        }
    }
}
