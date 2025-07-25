using System.Threading.Tasks;

namespace DispatcherWeb.WebHooks
{
    public interface IAppWebhookPublisher
    {
        Task PublishTestWebhook();
    }
}
