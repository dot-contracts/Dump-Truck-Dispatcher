using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
