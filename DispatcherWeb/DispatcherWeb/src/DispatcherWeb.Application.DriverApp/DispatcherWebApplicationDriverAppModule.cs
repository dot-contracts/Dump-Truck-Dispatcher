using Abp.Modules;
using Abp.Reflection.Extensions;

namespace DispatcherWeb.DriverApp
{
    [DependsOn(
        typeof(DispatcherWebApplicationSharedModule),
        typeof(DispatcherWebCoreModule)
        )]
    public class DispatcherWebApplicationDriverAppModule : AbpModule
    {
        public override void PreInitialize()
        {
        }

        public override void Initialize()
        {
            var assembly = typeof(DispatcherWebApplicationDriverAppModule).GetAssembly();
            IocManager.RegisterAssemblyByConvention(assembly);
        }
    }
}