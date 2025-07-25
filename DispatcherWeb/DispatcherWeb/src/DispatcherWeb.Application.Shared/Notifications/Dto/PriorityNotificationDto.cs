using System;
using Abp.Notifications;

namespace DispatcherWeb.Notifications.Dto
{
    public class PriorityNotificationDto
    {
        public Guid Id { get; set; }
        public NotificationSeverity Severity { get; set; }
        public string Message { get; set; }
    }
}
