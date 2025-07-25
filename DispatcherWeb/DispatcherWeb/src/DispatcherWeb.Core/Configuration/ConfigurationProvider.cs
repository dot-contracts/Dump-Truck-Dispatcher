using Abp;
using Abp.Configuration;
using Abp.Dependency;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Configuration
{
    public class AbpConfigurationProvider : IAbpConfigurationProvider, ISingletonDependency
    {
        public AbpConfigurationProvider(IAppConfigurationAccessor configurationAccessor)
        {
            Configuration = configurationAccessor.Configuration;
        }

        public IConfigurationRoot Configuration { get; }

        public string GetEncryptionKey()
        {
            return GetNotEmptyConfigurationValue(DispatcherWebConsts.EncryptionKey);
        }

        protected string GetNotEmptyConfigurationValue(string name)
        {
            var value = Configuration[name];

            if (string.IsNullOrEmpty(value))
            {
                throw new AbpException($"Configuration value for '{name}' is null or empty!");
            }

            return value;
        }
    }
}
