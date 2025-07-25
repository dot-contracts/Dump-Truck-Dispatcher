using System.IO;
using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.IO;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Web.Sessions;
using Abp.Web.Timing;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Storage;
using DispatcherWeb.Web.Areas.App.Startup;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.Timing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Web.Startup
{
    [DependsOn(
        typeof(DispatcherWebWebCoreModule)
    )]
    public class DispatcherWebWebMvcModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public DispatcherWebWebMvcModule(
            IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:WebSiteRootAddress"] ?? "https://localhost:44332/";
            Configuration.Navigation.Providers.Add<AppNavigationProvider>();

            Configuration.ReplaceService<ISessionScriptManager, DispatcherWebSessionScriptManager>();
            Configuration.ReplaceService<ITimingScriptManager, DispatcherWebTimingScriptManager>();
            Configuration.ReplaceService<IBinaryObjectManager, AzureBlobBinaryObjectManager>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebWebMvcModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            SetAppFolders();

            using (var scope = IocManager.CreateScope())
            {
                var databaseCheckHelper = scope.Resolve<DatabaseCheckHelper>();
#pragma warning disable CS0618 // Type or member is obsolete - ignore sync call for now
                if (_appConfiguration["App:CheckDbExistence"] == "true"
                    && !databaseCheckHelper.Exist(_appConfiguration["ConnectionStrings:Default"]))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return;
                }
            }

            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            if (_appConfiguration["App:HangfireServerEnabled"] == "true")
            {
                if (IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
                {
                    workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
                    workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
                }
                workManager.Add(IocManager.Resolve<DeferredBinaryObjectSyncBackgroundWorker>());
                if (_appConfiguration.ParseInt("SignalR:OnlineClientTimeout") > 0)
                {
                    workManager.Add(IocManager.Resolve<RemoveStaleOnlineClientsPeriodicJob>());
                }
            }
            //this should run on every instance to invalidate in-memory caches, so the value of App:HangfireServerEnabled is ignored
            if (_appConfiguration.ParseInt("CacheInvalidation:PeriodicCheckInterval") > 0
                && !string.IsNullOrEmpty(_appConfiguration["Abp:RedisCache:ConnectionString"]))
            {
                workManager.Add(IocManager.Resolve<CacheInvalidationPeriodicJob>());
            }
        }

        private void SetAppFolders()
        {
            var appFolders = IocManager.Resolve<AppFolders>();

            appFolders.SampleProfileImagesFolder = Path.Combine(_env.WebRootPath, @"Common\Images\SampleProfilePics");
            appFolders.TempFileDownloadFolder = Path.Combine(_env.WebRootPath, @"Temp\Downloads");
            appFolders.WebLogsFolder = Path.Combine(_env.ContentRootPath, @"App_Data\Logs");

            if (_env.IsDevelopment())
            {
                var currentAssemblyDirectoryPath = Assembly.GetExecutingAssembly().GetDirectoryPathOrNull();
                if (currentAssemblyDirectoryPath != null)
                {
                    appFolders.WebLogsFolder = Path.Combine(currentAssemblyDirectoryPath, @"App_Data\Logs");
                }
            }

            try
            {
                DirectoryHelper.CreateIfNotExists(appFolders.TempFileDownloadFolder);
            }
            catch { }
        }

    }
}
