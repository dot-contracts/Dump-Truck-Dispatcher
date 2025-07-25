using System.Threading.Tasks;
using Abp.Dependency;

namespace DispatcherWeb.Configuration
{
    public interface IAppSettingAvailabilityProvider : ITransientDependency
    {
        Task<bool> IsSettingAvailableAsync(string settingName);
    }
}
