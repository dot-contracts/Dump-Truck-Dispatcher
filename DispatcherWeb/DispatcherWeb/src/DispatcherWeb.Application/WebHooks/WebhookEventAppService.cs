using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Webhooks;
using DispatcherWeb.Authorization;

namespace DispatcherWeb.WebHooks
{
    [AbpAuthorize(AppPermissions.Pages_Administration_WebhookSubscription)]
    public class WebhookEventAppService : DispatcherWebAppServiceBase, IWebhookEventAppService
    {
        private readonly IWebhookEventStore _webhookEventStore;

        public WebhookEventAppService(IWebhookEventStore webhookEventStore)
        {
            _webhookEventStore = webhookEventStore;
        }

        public async Task<WebhookEvent> Get(string id)
        {
            return await _webhookEventStore.GetAsync(await AbpSession.GetTenantIdOrNullAsync(), Guid.Parse(id));
        }
    }
}
