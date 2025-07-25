using DispatcherWeb.WebPush;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class SendPushMessageToDriversSubscriptionDto
    {
        public int DriverId { get; set; }
        public int PushSubscriptionId { get; set; }
        public PushSubscriptionDto PushSubscription { get; set; }
    }
}
