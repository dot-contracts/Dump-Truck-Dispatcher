using Abp.Notifications;

namespace DispatcherWeb.Notifications
{
    public class SendPriorityNotificationInput
    {
        public string Message { get; set; }
        public NotificationSeverity Severity { get; set; }
        public bool? OnlineFilter { get; set; }
        public string[] RoleFilter { get; set; }
        public int[] OfficeIdFilter { get; set; }
        public long[] UserIdFilter { get; set; }

        public SendPriorityNotificationInput()
        {
        }

        public SendPriorityNotificationInput(string message, NotificationSeverity severity, params int[] officeIdFilter)
        {
            Message = message;
            Severity = severity;
            OfficeIdFilter = officeIdFilter;
        }
    }
}
