using System;
using System.Text;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Modules;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Web.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DispatcherWeb.Web.Startup
{
    public static class AuthConfigurer
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IAbpAntiForgeryManager, DispatcherWebAntiForgeryManager>();
            services.AddScoped<IPermissionChecker, PermissionChecker>();
            services.AddScoped<IBinaryObjectManager, AzureBlobBinaryObjectManager>();

            services.AddIdentity<User, Role>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<DispatcherWebDbContext>()
            .AddUserManager<UserManager>()
            .AddRoleManager<RoleManager>()
            .AddSignInManager<SignInManager>()
            .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
            });

            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            if (bool.Parse(configuration["Authentication:JwtBearer:IsEnabled"]))
            {
                authenticationBuilder.AddJwtBearer(options =>
                {
                    options.UseSecurityTokenValidators = true;
                    options.Authority = configuration["Authentication:JwtBearer:Issuer"];
                    options.Audience = configuration["Authentication:JwtBearer:Audience"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = bool.Parse(configuration["Authentication:JwtBearer:ValidateIssuer"]),
                        ValidIssuer = configuration["Authentication:JwtBearer:Issuer"],
                        ValidateAudience = bool.Parse(configuration["Authentication:JwtBearer:ValidateAudience"]),
                        ValidAudience = configuration["Authentication:JwtBearer:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                });
            }

            if (bool.Parse(configuration["IdentityServer:IsEnabled"]))
            {
                var identityServerBuilder = services.AddIdentityServer(options =>
                {
                    options.IssuerUri = configuration["IdentityServer:IssuerUri"];
                })
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
                .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
                .AddInMemoryClients(IdentityServerConfig.GetClients(configuration))
                .AddAbpPersistedGrants<DispatcherWebDbContext>()
                .AddAbpIdentityServer<DispatcherWebDbContext>();

                if (!bool.Parse(configuration["IdentityServer:IsEnabled"]))
                {
                    identityServerBuilder.AddDeveloperSigningCredential();
                }

                services.AddAuthentication()
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = configuration["IdentityServer:IssuerUri"];
                        options.SupportedTokens = IdentityServer4.AccessTokenValidation.SupportedTokens.Both;
                        options.ApiSecret = "secret";
                        options.ApiName = "default-api";
                        options.RequireHttpsMetadata = false;
                        options.InMemoryJwtTokens = true;
                    });
            }
        }
    }
}
