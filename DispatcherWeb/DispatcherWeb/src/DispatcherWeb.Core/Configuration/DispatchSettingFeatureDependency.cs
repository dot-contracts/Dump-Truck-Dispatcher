using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;

namespace DispatcherWeb.Configuration
{
    public class DispatchSettingFeatureDependency : IFeatureDependency
    {
        public async Task<bool> IsSatisfiedAsync(IFeatureDependencyContext context)
        {
            var settingManager = context.IocResolver.Resolve<ISettingManager>();
            try
            {
                var dispatchVia = (DispatchVia)await settingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia);
                return dispatchVia != DispatchVia.None;
            }
            finally
            {
                context.IocResolver.Release(settingManager);
            }
        }
    }
}
