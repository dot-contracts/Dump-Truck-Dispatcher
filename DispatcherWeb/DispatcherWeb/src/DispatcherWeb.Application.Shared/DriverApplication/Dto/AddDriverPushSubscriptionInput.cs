using DispatcherWeb.WebPush;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class AddDriverPushSubscriptionInput
    {
        public PushSubscriptionDto PushSubscription { get; set; }
        public int DriverId { get; set; }
        public int? DeviceId { get; set; }
    }
}
