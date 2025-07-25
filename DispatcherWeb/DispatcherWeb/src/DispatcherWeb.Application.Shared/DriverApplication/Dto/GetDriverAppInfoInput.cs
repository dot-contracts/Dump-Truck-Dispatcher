using DispatcherWeb.WebPush;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class GetDriverAppInfoInput
    {
        public bool RequestNewDeviceId { get; set; }
        public string Useragent { get; set; }
        public int? DeviceId { get; set; }
        public MobilePlatform? MobilePlatform { get; set; }
        public PushSubscriptionDto PushSubscription { get; set; }
        public string AppVersion { get; set; }
    }
}
