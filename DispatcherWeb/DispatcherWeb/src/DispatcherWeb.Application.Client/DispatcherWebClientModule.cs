using Abp.Modules;
using Abp.Reflection.Extensions;

namespace DispatcherWeb
{
    public class DispatcherWebClientModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebClientModule).GetAssembly());
        }
    }
}
