using Abp.Notifications;

namespace DispatcherWeb.Infrastructure.Notifications
{
    public class ImportCompletedNotificationData : NotificationData
    {
        public ImportCompletedNotificationData(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
    }
}
