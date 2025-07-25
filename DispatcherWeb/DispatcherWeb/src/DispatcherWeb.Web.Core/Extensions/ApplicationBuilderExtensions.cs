using System;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;
using Abp.Runtime.Caching.Redis;
using DispatcherWeb.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace DispatcherWeb.Web.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDispatcherWebForwardedHeaders(this IApplicationBuilder builder)
        {
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            return builder.UseForwardedHeaders(options);
        }

        public static void UseRedisInvalidatableCache(
            this ICachingConfiguration cachingConfiguration,
            Action<AbpRedisCacheOptions> optionsAction)
        {
            var iocManager = cachingConfiguration.AbpConfiguration.IocManager;

            iocManager.RegisterIfNot<ICacheManager, RedisInvalidatableInMemoryCacheManager>();

            optionsAction(iocManager.Resolve<AbpRedisCacheOptions>());
        }
    }
}
