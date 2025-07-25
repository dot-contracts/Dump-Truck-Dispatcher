using System;
using Abp.IdentityServer4vNext;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using IdentityServer4.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DispatcherWeb.Web.IdentityServer
{
    public static class IdentityServerRegistrar
    {
        public static void Register(IServiceCollection services, IConfigurationRoot configuration, Action<IdentityServerOptions> setupOptions)
        {
            services.AddIdentityServer(setupOptions)
                .AddSigningCredential(configuration)
                .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
                .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources(configuration))
                .AddInMemoryClients(IdentityServerConfig.GetClients(configuration))
                .AddAbpPersistedGrants<DispatcherWebDbContext>()
                .AddAbpIdentityServer<User, Tenant, Role>();
        }

        public static void Register(IServiceCollection services, IConfigurationRoot configuration)
        {
            Register(services, configuration, options => { });
        }

        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, IConfigurationRoot configuration)
        {
            var signingKey = configuration["IdentityServer:SigningKey"];
            if (string.IsNullOrEmpty(signingKey))
            {
                return builder.AddDeveloperSigningCredential();
            }
            else
            {
                var jwk = new JsonWebKey(signingKey);
                return builder.AddSigningCredential(jwk, jwk.Alg);
            }
        }
    }
}
