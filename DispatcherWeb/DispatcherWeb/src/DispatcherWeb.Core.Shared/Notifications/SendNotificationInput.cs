using Abp.Notifications;

namespace DispatcherWeb.Notifications
{
    public class SendNotificationInput
    {
        public SendNotificationInput(string notificationName, string message, NotificationSeverity severity)
        {
            NotificationName = notificationName;
            Message = message;
            Severity = severity;
        }

        public string NotificationName { get; set; }
        public string Message { get; set; }
        public NotificationSeverity Severity { get; set; }
        public string[] RoleFilter { get; set; }
        public string PermissionFilter { get; set; }
        public int[] OfficeIdFilter { get; set; }
        public bool IncludeHostUsers { get; set; }
        public bool IncludeLocalUsers { get; set; }
        public bool? OnlineFilter { get; set; }

    }
}
