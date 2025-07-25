using Abp.Modules;
using Abp.Reflection.Extensions;

namespace DispatcherWeb
{
    public class DispatcherWebCoreSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebCoreSharedModule).GetAssembly());
        }
    }
}