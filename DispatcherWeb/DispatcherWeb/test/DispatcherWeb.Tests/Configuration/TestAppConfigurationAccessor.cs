using Abp.Dependency;
using Abp.Reflection.Extensions;
using DispatcherWeb.Configuration;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Tests.Configuration
{
    public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
    {
        public IConfigurationRoot Configuration { get; }

        public TestAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(
                typeof(DispatcherWebTestModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }
    }
}
