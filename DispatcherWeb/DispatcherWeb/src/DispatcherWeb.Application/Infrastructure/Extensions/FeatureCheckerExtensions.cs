using System.Threading.Tasks;
using Abp.Application.Features;
using DispatcherWeb.Features;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class FeatureCheckerExtensions
    {
        public static async Task<bool> AllowLeaseHaulersFeature(this IFeatureChecker featureChecker)
        {
            return await featureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature);
        }

        public static async Task<bool> AllowMultiOfficeFeature(this IFeatureChecker featureChecker)
        {
            return await featureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature);
        }
    }
}
