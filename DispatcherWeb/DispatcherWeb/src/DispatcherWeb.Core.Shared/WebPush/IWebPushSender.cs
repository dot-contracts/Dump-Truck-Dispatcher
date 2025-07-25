using System.Threading.Tasks;

namespace DispatcherWeb.WebPush
{
    public interface IWebPushSender
    {
        Task SendAsync(PushSubscriptionDto pushSubscription, DriverApplication.PwaPushMessage payload);
        Task SendAsync(PushSubscriptionDto pushSubscription, string payload);
        Task SendAsync(PushSubscriptionDto pushSubscription, object payload);
    }
}
