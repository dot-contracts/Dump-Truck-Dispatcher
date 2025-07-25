using System;
using Abp;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Castle.MicroKernel.Registration;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.Test.Base.Configuration;
using DispatcherWeb.Test.Base.DependencyInjection;
using DispatcherWeb.Test.Base.Sessions;
using DispatcherWeb.Test.Base.Url;
using DispatcherWeb.Test.Base.Web;
using DispatcherWeb.Url;
using NSubstitute;

namespace DispatcherWeb.Test.Base
{
    [DependsOn(
        typeof(DispatcherWebApplicationModule),
        typeof(DispatcherWebEntityFrameworkCoreModule),
        typeof(AbpTestBaseModule))]
    public class DispatcherWebTestBaseModule : AbpModule
    {
        public DispatcherWebTestBaseModule(DispatcherWebEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule)
        {
            abpZeroTemplateEntityFrameworkCoreModule.SkipDbContextRegistration = true;
        }

        public override void PreInitialize()
        {
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator>();

            IocManager.Register<IAppUrlService, FakeAppUrlService>();
            IocManager.Register<IWebUrlService, FakeWebUrlService>();
            IocManager.Register<IRecaptchaValidator, FakeRecaptchaValidator>();

            Configuration.ReplaceService<IAppConfigurationAccessor, TestAppConfigurationAccessor>();
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);

            IocManager.IocContainer.Register(
                Component.For<IAbpSession, IExtendedAbpSession, ExtendedTestAbpSession>()
                    .ImplementedBy<ExtendedTestAbpSession>()
                    .LifestyleSingleton()
                    .IsDefault()
            );

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
