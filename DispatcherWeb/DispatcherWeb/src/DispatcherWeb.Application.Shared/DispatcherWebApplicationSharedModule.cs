using Abp.Modules;
using Abp.Reflection.Extensions;

namespace DispatcherWeb
{
    [DependsOn(typeof(DispatcherWebCoreSharedModule))]
    public class DispatcherWebApplicationSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebApplicationSharedModule).GetAssembly());
        }
    }
}