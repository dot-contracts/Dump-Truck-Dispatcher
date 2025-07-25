using System;
using System.Net.Mail;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using DispatcherWeb.Authorization;
using DispatcherWeb.CustomerNotifications.Cache;
using DispatcherWeb.Debugging;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Infrastructure.EntityUpdaters;
using DispatcherWeb.Infrastructure.Sms;

namespace DispatcherWeb
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(
        typeof(DispatcherWebApplicationSharedModule),
        typeof(DispatcherWebCoreModule)
        )]
    public class DispatcherWebApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

            Configuration.Caching.Configure(CustomerNotificationCache.DetailsCacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromHours(24);
            });
            Configuration.Caching.Configure(CustomerNotificationCache.IdsCacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromHours(8);
            });
            ConfigureCaches();

            Configuration.Validation.IgnoredTypes.Add(typeof(MailMessage));

            if (DebugHelper.IsDebug)
            {
                //Disabling SMS sending in debug mode
                Configuration.ReplaceService<ISmsSender, NullSmsSender>(DependencyLifeStyle.Transient);
                Configuration.ReplaceService<ITrackableEmailSender, NullTrackableEmailSender>(DependencyLifeStyle.Transient);
            }
        }

        private void ConfigureCaches()
        {
            Configuration.Caching.ConfigureAll(cache =>
                cache.DefaultSlidingExpireTime = TimeSpan.FromHours(24));
        }

        public override void Initialize()
        {
            var assembly = typeof(DispatcherWebApplicationModule).GetAssembly();
            IocManager.RegisterAssemblyByConvention(assembly);

            IocManager.IocContainer.Register(
                Classes.FromAssembly(assembly)
                    .BasedOn(typeof(IEntityUpdaterFactory<>))
                    .WithService.Base()
                    .Configure(configurer => configurer.Named(Guid.NewGuid().ToString()))
            );

            IocManager.IocContainer.Register(
                Classes.FromAssembly(assembly)
                    .BasedOn(typeof(IReadonlyCheckerFactory<>))
                    .WithService.Base()
                    .Configure(configurer => configurer.Named(Guid.NewGuid().ToString()))
            );
        }
    }
}
