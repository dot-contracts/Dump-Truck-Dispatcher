using Abp.Modules;
using Abp.Reflection.Extensions;

namespace DispatcherWeb.ActiveReports
{
    [DependsOn(
        typeof(DispatcherWebApplicationSharedModule),
        typeof(DispatcherWebCoreModule)
        )]
    public class DispatcherWebApplicationActiveReportsModule : AbpModule
    {
        public override void PreInitialize()
        {
        }

        public override void Initialize()
        {
            var assembly = typeof(DispatcherWebApplicationActiveReportsModule).GetAssembly();
            IocManager.RegisterAssemblyByConvention(assembly);
        }
    }
}