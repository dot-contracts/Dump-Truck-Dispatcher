using System.Threading.Tasks;
using Abp.Webhooks;

namespace DispatcherWeb.WebHooks
{
    public interface IWebhookEventAppService
    {
        Task<WebhookEvent> Get(string id);
    }
}
