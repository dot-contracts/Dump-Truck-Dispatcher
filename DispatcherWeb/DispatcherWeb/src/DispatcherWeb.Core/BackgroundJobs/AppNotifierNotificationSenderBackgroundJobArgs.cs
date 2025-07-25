using Abp;
using DispatcherWeb.Notifications;

namespace DispatcherWeb.BackgroundJobs
{
    public class AppNotifierNotificationSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public SendNotificationInput SendNotificationInput { get; set; }
    }
}
