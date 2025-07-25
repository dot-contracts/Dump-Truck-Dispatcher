using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.UI;
using DispatcherWeb.Features;

namespace DispatcherWeb.Chat
{
    public class ChatFeatureChecker : DispatcherWebDomainServiceBase, IChatFeatureChecker
    {
        private readonly IFeatureChecker _featureChecker;

        public ChatFeatureChecker(
            IFeatureChecker featureChecker
        )
        {
            _featureChecker = featureChecker;
        }

        public async Task CheckChatFeaturesAsync(int? sourceTenantId, int? targetTenantId)
        {
            await CheckChatFeaturesInternalAsync(sourceTenantId, targetTenantId, ChatSide.Sender);
            await CheckChatFeaturesInternalAsync(targetTenantId, sourceTenantId, ChatSide.Receiver);
        }

        private async Task CheckChatFeaturesInternalAsync(int? sourceTenantId, int? targetTenantId, ChatSide side)
        {
            var localizationSuffix = side == ChatSide.Sender ? "ForSender" : "ForReceiver";
            if (sourceTenantId.HasValue)
            {
                if (!await _featureChecker.IsEnabledAsync(sourceTenantId.Value, AppFeatures.ChatFeature))
                {
                    throw new UserFriendlyException(L("ChatFeatureIsNotEnabled" + localizationSuffix));
                }

                if (targetTenantId.HasValue)
                {
                    if (sourceTenantId == targetTenantId)
                    {
                        return;
                    }

                    if (!await _featureChecker.IsEnabledAsync(sourceTenantId.Value, AppFeatures.TenantToTenantChatFeature))
                    {
                        throw new UserFriendlyException(L("TenantToTenantChatFeatureIsNotEnabled" + localizationSuffix));
                    }
                }
                else
                {
                    if (!await _featureChecker.IsEnabledAsync(sourceTenantId.Value, AppFeatures.TenantToHostChatFeature))
                    {
                        throw new UserFriendlyException(L("TenantToHostChatFeatureIsNotEnabled" + localizationSuffix));
                    }
                }
            }
            else
            {
                if (targetTenantId.HasValue)
                {
                    if (!await _featureChecker.IsEnabledAsync(targetTenantId.Value, AppFeatures.TenantToHostChatFeature))
                    {
                        throw new UserFriendlyException(L("TenantToHostChatFeatureIsNotEnabled" + (side == ChatSide.Sender ? "ForReceiver" : "ForSender")));
                    }
                }
            }
        }
    }
}
