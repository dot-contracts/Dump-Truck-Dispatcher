using System.Collections.Concurrent;
using Abp.Extensions;
using Abp.Reflection.Extensions;
using DispatcherWeb.Web;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Configuration
{
    public static class AppConfigurations
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> ConfigurationCache;

        static AppConfigurations()
        {
            ConfigurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }
        public static IConfigurationRoot GetForEnvironment(string envrionmentName)
        {
            return Get(
                WebContentDirectoryFinder.CalculateContentRootFolder(),
                envrionmentName
                );
        }

        public static IConfigurationRoot Get(string path, string environmentName = null, bool addUserSecrets = false)
        {
            var cacheKey = path + "#" + environmentName + "#" + addUserSecrets;
            return ConfigurationCache.GetOrAdd(
                cacheKey,
                _ => BuildConfiguration(path, environmentName, addUserSecrets)
            );
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null,
            bool addUserSecrets = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!environmentName.IsNullOrWhiteSpace())
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            }

            builder = builder.AddEnvironmentVariables();

            if (addUserSecrets)
            {
                builder.AddUserSecrets(typeof(AppConfigurations).GetAssembly(), true);
            }

            var builtConfig = builder.Build();
            new AppAzureKeyVaultConfigurer().Configure(builder, builtConfig);

            return builder.Build();
        }

        public static bool IsMultitenancyEnabled(this IConfigurationRoot appConfiguration)
        {
            return appConfiguration["Abp:MultitenancyIsEnabled"]?.ToLower() == "true";
        }

        public static string GetCookieDomain(this IConfigurationRoot configuration)
        {
            var cookieDomain = configuration["App:CookieDomain"];
            if (string.IsNullOrEmpty(cookieDomain))
            {
                cookieDomain = null;
            }

            return cookieDomain;
        }

        public static int ParseInt(this IConfigurationRoot configuration, string name, int defaultValue)
        {
            var stringValue = configuration[name];
            if (!string.IsNullOrEmpty(stringValue)
                && int.TryParse(stringValue, out var parsedValue))
            {
                return parsedValue;
            }
            return defaultValue;
        }

        public static int ParseInt(this IConfigurationRoot configuration, string name)
        {
            return configuration.ParseInt(name, 0);
        }

        public static int GetCacheInvalidationDebounceMs(this IConfigurationRoot configuration)
        {
            return configuration.ParseInt("CacheInvalidation:DebounceMs", 300);
        }

        public static int GetCacheInvalidationNetworkDelayMs(this IConfigurationRoot configuration)
        {
            return configuration.ParseInt("CacheInvalidation:NetworkDelayMs", 500);
        }

        public static int GetCombinedCacheInvalidationDelay(this IConfigurationRoot configuration)
        {
            return configuration.GetCacheInvalidationDebounceMs() + configuration.GetCacheInvalidationNetworkDelayMs();
        }
    }
}
