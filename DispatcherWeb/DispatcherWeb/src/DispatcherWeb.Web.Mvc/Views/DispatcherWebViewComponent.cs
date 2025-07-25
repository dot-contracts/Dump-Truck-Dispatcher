using Abp.AspNetCore.Mvc.ViewComponents;

namespace DispatcherWeb.Web.Views
{
    public abstract class DispatcherWebViewComponent : AbpViewComponent
    {
        protected DispatcherWebViewComponent()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }
    }
}