using System.Threading.Tasks;

namespace DispatcherWeb.Chat
{
    public interface IChatFeatureChecker
    {
        Task CheckChatFeaturesAsync(int? sourceTenantId, int? targetTenantId);
    }
}
