using Abp;
using Abp.Domain.Entities;
using Abp.Notifications;

namespace DispatcherWeb.BackgroundJobs
{
    public class NotificationPublisherBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }

        public string NotificationName { get; set; }

        public string NotificationDataString { get; set; }

        public EntityIdentifier EntityIdentifier { get; set; }

        public NotificationSeverity Severity { get; set; }

        public UserIdentifier[] UserIds { get; set; }

        public UserIdentifier[] ExcludedUserIds { get; set; }

        public int?[] TenantIds { get; set; }
    }
}
