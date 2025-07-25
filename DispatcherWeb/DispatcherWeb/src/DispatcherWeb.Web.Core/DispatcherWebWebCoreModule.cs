using System.IO;
using System.Text;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.MultiTenancy;
using Abp.AspNetCore.SignalR;
using Abp.BackgroundJobs;
using Abp.Configuration.Startup;
using Abp.Hangfire;
using Abp.Hangfire.Configuration;
using Abp.Modules;
using Abp.Notifications;
using Abp.RealTime;
using Abp.Reflection.Extensions;
using Abp.Runtime.Caching.Redis;
using Abp.Zero.Configuration;
using Castle.MicroKernel.Registration;
using DispatcherWeb.ActiveReports;
using DispatcherWeb.Authentication.TwoFactor;
using DispatcherWeb.AzureServiceBus;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.SignalR;
using DispatcherWeb.Web.Authentication.JwtBearer;
using DispatcherWeb.Web.Common;
using DispatcherWeb.Web.Configuration;
using DispatcherWeb.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AbpSignalRRealTimeNotifier = Abp.AspNetCore.SignalR.Notifications.SignalRRealTimeNotifier;
using DispatcherWebSignalRRealTimeNotifier = DispatcherWeb.Web.SignalR.SignalRRealTimeNotifier;

namespace DispatcherWeb.Web
{
    [DependsOn(
        typeof(DispatcherWebApplicationModule),
        typeof(DispatcherWebApplicationDriverAppModule),
        typeof(DispatcherWebApplicationActiveReportsModule),
        typeof(DispatcherWebEntityFrameworkCoreModule),
        typeof(AbpAspNetCoreModule),
        typeof(AbpAspNetCoreSignalRModule),
        typeof(AbpRedisCacheModule), //AbpRedisCacheModule dependency (and Abp.RedisCache nuget package) can be removed if not using Redis cache
        typeof(AbpHangfireAspNetCoreModule) //AbpHangfireModule dependency (and Abp.Hangfire.AspNetCore nuget package) can be removed if not using Hangfire
    )]
    public class DispatcherWebWebCoreModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public DispatcherWebWebCoreModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            //Set default connection string
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                DispatcherWebConsts.ConnectionStringName
            );

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(DispatcherWebApplicationModule).GetAssembly()
                );

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(DispatcherWebApplicationDriverAppModule).GetAssembly(),
                    moduleName: "driverApp",
                    useConventionalHttpVerbs: true
                );

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(DispatcherWebApplicationActiveReportsModule).GetAssembly(),
                    moduleName: "activeReports",
                    useConventionalHttpVerbs: true
                );

            Configuration.Caching.Configure(TwoFactorCodeCacheItem.CacheName,
                cache =>
                {
                    cache.DefaultSlidingExpireTime = TwoFactorCodeCacheItem.DefaultSlidingExpireTime;
                });

            if (_appConfiguration["Authentication:JwtBearer:IsEnabled"] != null
                && bool.Parse(_appConfiguration["Authentication:JwtBearer:IsEnabled"]))
            {
                ConfigureTokenAuth();
            }

            Configuration.MultiTenancy.Resolvers.Remove<DomainTenantResolveContributor>();

            Configuration.ReplaceService<IAppConfigurationAccessor, AppConfigurationAccessor>();

            Configuration.ReplaceService<IAppConfigurationWriter, AppConfigurationWriter>();

            if (!string.IsNullOrEmpty(_appConfiguration["Abp:ServiceBusConnectionString"])
                && !string.IsNullOrEmpty(_appConfiguration["Abp:ServiceBusBackgroundJobQueueName"])
                && _appConfiguration["Abp:UseServiceBusBackgroundJobManager"] == "true"
            )
            {
                Configuration.ReplaceService<IBackgroundJobManager, ServiceBusBackgroundJobManager>();
            }
            else if (WebConsts.HangfireDashboardEnabled)
            {
                Configuration.BackgroundJobs.UseHangfire();
            }

            var redisConnectionString = _appConfiguration["Abp:RedisCache:ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                //Use RedisOnlineClientStore

                IocManager.IocContainer.Register(
                    Component.For<IOnlineClientStore, IAsyncOnlineClientStore, RedisOnlineClientStore>()
                        .ImplementedBy<RedisOnlineClientStore>()
                        .LifestyleSingleton()
                        .IsDefault()
                );

                IocManager.IocContainer.Register(
                    Component.For(typeof(IOnlineClientStore<>), typeof(RedisOnlineClientStore<>))
                        .ImplementedBy(typeof(RedisOnlineClientStore<>))
                        .LifestyleSingleton()
                        .IsDefault()
                );
            }
            else
            {
                //Use InMemoryAsyncOnlineClientStore

                IocManager.IocContainer.Register(
                    Component.For<IOnlineClientStore, IAsyncOnlineClientStore, InMemoryAsyncOnlineClientStore>()
                        .ImplementedBy<InMemoryAsyncOnlineClientStore>()
                        .LifestyleSingleton()
                        .IsDefault()
                );

                IocManager.IocContainer.Register(
                    Component.For(typeof(IOnlineClientStore<>), typeof(InMemoryAsyncOnlineClientStore<>))
                        .ImplementedBy(typeof(InMemoryAsyncOnlineClientStore<>))
                        .LifestyleSingleton()
                        .IsDefault()
                );
            }

            Configuration.Caching.UseRedisInvalidatableCache(options =>
            {
                options.ConnectionString = redisConnectionString;
                options.DatabaseId = _appConfiguration.GetValue<int>("Abp:RedisCache:DatabaseId");
            });

            IocManager.IocContainer.Register(
                Component.For<IOnlineClientManager, IAsyncOnlineClientManager, AsyncOnlineClientManager>()
                    .ImplementedBy<AsyncOnlineClientManager>()
                    .LifestyleSingleton()
                    .IsDefault()
            );

            IocManager.IocContainer.Register(
                Component.For(typeof(IOnlineClientManager<>), typeof(AsyncOnlineClientManager<>))
                    .ImplementedBy(typeof(AsyncOnlineClientManager<>))
                    .LifestyleSingleton()
                    .IsDefault()
            );

            Configuration.ReplaceService<IRealTimeNotifier, DispatcherWebSignalRRealTimeNotifier>();
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey =
                new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials =
                new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.AccessTokenExpiration = AppConsts.AccessTokenExpiration;
            tokenAuthConfig.RefreshTokenExpiration = AppConsts.RefreshTokenExpiration;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebWebCoreModule).GetAssembly());

            Configuration.Notifications.Notifiers.Remove<AbpSignalRRealTimeNotifier>();
            Configuration.Notifications.Notifiers.Add<DispatcherWebSignalRRealTimeNotifier>();
        }

        public override void PostInitialize()
        {
            SetAppFolders();
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(DispatcherWebWebCoreModule).Assembly);
        }

        private void SetAppFolders()
        {
            var appFolders = IocManager.Resolve<AppFolders>();

            appFolders.SampleProfileImagesFolder = Path.Combine(_env.WebRootPath,
                $"Common{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}SampleProfilePics");
            appFolders.WebLogsFolder = Path.Combine(_env.ContentRootPath, $"App_Data{Path.DirectorySeparatorChar}Logs");
        }
    }
}
