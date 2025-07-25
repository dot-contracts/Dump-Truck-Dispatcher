using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace DispatcherWeb.Web.Views
{
    public abstract class DispatcherWebRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected DispatcherWebRazorPage()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected async Task<bool> IsGrantedAsync(string permissionName)
        {
            return await PermissionChecker.IsGrantedAsync(permissionName);
        }
    }
}
