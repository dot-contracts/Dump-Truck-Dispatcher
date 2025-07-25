using Abp;
using DispatcherWeb.Notifications;

namespace DispatcherWeb.BackgroundJobs
{
    public class AppNotifierPriorityNotificationSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public SendPriorityNotificationInput SendPriorityNotificationInput { get; set; }
    }
}
