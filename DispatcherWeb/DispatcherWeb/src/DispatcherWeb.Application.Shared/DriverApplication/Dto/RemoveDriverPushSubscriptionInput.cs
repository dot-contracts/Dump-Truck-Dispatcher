using DispatcherWeb.WebPush;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class RemoveDriverPushSubscriptionInput
    {
        public PushSubscriptionDto PushSubscription { get; set; }
        public int DriverId { get; set; }
        public int? DeviceId { get; set; }
    }
}
