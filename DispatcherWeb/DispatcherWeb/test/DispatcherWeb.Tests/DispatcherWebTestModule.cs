using System;
using Abp;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.RealTime;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.SignalR;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Test.Base.Sessions;
using DispatcherWeb.Tests.Configuration;
using DispatcherWeb.Tests.DependencyInjection;
using DispatcherWeb.Tests.SignalR;
using DispatcherWeb.Tests.TestInfrastructure;
using DispatcherWeb.Tests.Url;
using DispatcherWeb.Tests.Web;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace DispatcherWeb.Tests
{
    [DependsOn(
        typeof(DispatcherWebApplicationModule),
        typeof(DispatcherWebEntityFrameworkCoreModule),
        typeof(AbpTestBaseModule))]
    public class DispatcherWebTestModule : AbpModule
    {
        public DispatcherWebTestModule(
            DispatcherWebEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule,
            IIocManager iocManager
        )
        {
            iocManager.Register<IWebHostEnvironment, FakeHostingEnvironment>();
            iocManager.Register<ILogger, NullLogger>();
            abpZeroTemplateEntityFrameworkCoreModule.SkipDbContextRegistration = true;
        }

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator>();

            IocManager.Register<IAppUrlService, FakeAppUrlService>();
            IocManager.Register<IWebUrlService, FakeWebUrlService>();
            IocManager.Register<IRecaptchaValidator, FakeRecaptchaValidator>();
            IocManager.Register<ISignalRCommunicator, FakeSignalRCommunicator>();

            Configuration.ReplaceService<IAppConfigurationAccessor, TestAppConfigurationAccessor>();
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);

            IocManager.IocContainer.Register(
                Component.For<IAbpSession, IExtendedAbpSession, ExtendedTestAbpSession>()
                    .ImplementedBy<ExtendedTestAbpSession>()
                    .LifestyleSingleton()
                    .IsDefault()
            );

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

            IocManager.RegisterIfNot<ICacheManager, RedisInvalidatableInMemoryCacheManager>();
            IocManager.RegisterIfNot<ICacheInvalidationService, NullCacheInvalidationService>();
            IocManager.RegisterIfNot<IDriverSyncRequestStore, NullDriverSyncRequestStore>();

            Configuration.ReplaceService<IRealTimeNotifier, DispatcherWeb.Web.SignalR.SignalRRealTimeNotifier>();

            //Uncomment below line to write change logs for the entities below:
            Configuration.EntityHistory.IsEnabled = true;
            Configuration.EntityHistory.Selectors.Add("DispatcherWebEntities", typeof(User), typeof(Tenant));
        }

        public override void Initialize()
        {
            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>()
            where TService : class
        {
            IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton()
            );
        }
    }
}
