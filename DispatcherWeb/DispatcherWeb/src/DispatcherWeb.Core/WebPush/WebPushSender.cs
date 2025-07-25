using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebPushLib = WebPush;

namespace DispatcherWeb.WebPush
{
    public class WebPushSender : IWebPushSender, ISingletonDependency
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly WebPushLib.VapidDetails _vapidDetails;
        private readonly WebPushLib.WebPushClient _webPushClient;

        public WebPushSender(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
            _vapidDetails = new WebPushLib.VapidDetails(
                _appConfiguration["WebPush:ContactLink"],
                _appConfiguration["WebPush:ServerPublicKey"],
                _appConfiguration["WebPush:ServerPrivateKey"]);

            _webPushClient = new WebPushLib.WebPushClient();
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, DriverApplication.PwaPushMessage payload)
        {
            await SendAsync(pushSubscriptionDto, JsonConvert.SerializeObject(payload));
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, object payload)
        {
            await SendAsync(pushSubscriptionDto, JsonConvert.SerializeObject(payload));
        }

        public async Task SendAsync(PushSubscriptionDto pushSubscriptionDto, string payload)
        {
            var pushSubscription = new WebPushLib.PushSubscription(pushSubscriptionDto.Endpoint, pushSubscriptionDto.Keys.P256dh, pushSubscriptionDto.Keys.Auth);
            await _webPushClient.SendNotificationAsync(pushSubscription, payload, _vapidDetails);
        }
    }
}
