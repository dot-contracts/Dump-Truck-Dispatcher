using System;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MailKit;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using Castle.MicroKernel.Registration;
using DispatcherWeb.Authorization.Delegation;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Chat;
using DispatcherWeb.Configuration;
using DispatcherWeb.DashboardCustomization.Definitions;
using DispatcherWeb.Debugging;
using DispatcherWeb.DynamicEntityProperties;
using DispatcherWeb.Emailing;
using DispatcherWeb.Features;
using DispatcherWeb.Friendships;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.Localization;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using DispatcherWeb.Notifications.Cache;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Timing;
using DispatcherWeb.WebHooks;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;

namespace DispatcherWeb
{
    [DependsOn(
        typeof(DispatcherWebCoreSharedModule),
        typeof(AbpZeroCoreModule),
        typeof(AbpMailKitModule))]
    public class DispatcherWebCoreModule : AbpModule
    {

        public override void PreInitialize()
        {
            var env = IocManager.Resolve<IWebHostEnvironment>();
            var appConfiguration = env.GetAppConfiguration();

            //workaround for issue: https://github.com/aspnet/EntityFrameworkCore/issues/9825
            //related github issue: https://github.com/aspnet/EntityFrameworkCore/issues/10407
            AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue9825", true);

            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            //Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            DispatcherWebLocalizationConfigurer.Configure(Configuration.Localization);

            //Adding feature providers
            Configuration.Features.Providers.Add<AppFeatureProvider>();

            //Adding setting providers
            Configuration.Settings.Providers.Add<AppSettingProvider>();

            //Adding notification providers
            Configuration.Notifications.Providers.Add<AppNotificationProvider>();

            //Adding webhook definition providers
            Configuration.Webhooks.Providers.Add<AppWebhookDefinitionProvider>();
            Configuration.Webhooks.TimeoutDuration = TimeSpan.FromMinutes(1);
            Configuration.Webhooks.IsAutomaticSubscriptionDeactivationEnabled = false;

            //Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = appConfiguration.IsMultitenancyEnabled();

            //Twilio - Enable this line to activate Twilio SMS integration
            //Configuration.ReplaceService<ISmsSender,TwilioSmsSender>();

            //Adding DynamicEntityParameters definition providers
            Configuration.DynamicEntityProperties.Providers.Add<AppDynamicEntityPropertyDefinitionProvider>();

            // MailKit configuration
            Configuration.Modules.AbpMailKit().SecureSocketOption = SecureSocketOptions.Auto;
            Configuration.ReplaceService<IMailKitSmtpBuilder, DispatcherWebMailKitSmtpBuilder>(DependencyLifeStyle.Transient);

            //Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            if (DebugHelper.IsDebug)
            {
                //Disabling email sending in debug mode
                Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            }

            Configuration.ReplaceService<Abp.Notifications.INotificationStore, DispatcherWebNotificationStore>();
            Configuration.ReplaceService<Abp.Notifications.INotificationPublisher, DispatcherWebNotificationPublisher>();

            IocManager.IocContainer.Register(
                Component.For<IAbpSession, IExtendedAbpSession, AspNetZeroAbpSession>()
                    .ImplementedBy<AspNetZeroAbpSession>()
                    .LifestyleSingleton()
                    .IsDefault()
            );

            Configuration.Caching.Configure(FriendCacheItem.CacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(30);
            });

            Configuration.Caching.Configure(Top3UserNotificationCache.CacheNameConst, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(30);
            });

            Configuration.Caching.Configure(PriorityUserNotificationCache.CacheNameConst, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(30);
            });

            IocManager.Register<DashboardConfiguration>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            var configurationAccessor = IocManager.Resolve<IAppConfigurationAccessor>();
            var configuration = configurationAccessor.Configuration;
            var enableChatUserStateWatcher = configuration["App:EnableChatUserStateWatcher"] == "true"
                && configuration["App:SignalRServerEnabled"] == "true";

            IocManager.RegisterIfNot<IChatCommunicator, NullChatCommunicator>();
            IocManager.Register<IUserDelegationConfiguration, UserDelegationConfiguration>();

            if (enableChatUserStateWatcher)
            {
                IocManager.Resolve<ChatUserStateWatcher>().Initialize();
            }
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
