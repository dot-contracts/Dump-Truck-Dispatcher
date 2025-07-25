using System.Threading.Tasks;
using DispatcherWeb.Web.Session;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Views.Shared.Components.TenantChange
{
    public class TenantChangeViewComponent : DispatcherWebViewComponent
    {
        private readonly IPerRequestSessionCache _sessionCache;

        public TenantChangeViewComponent(IPerRequestSessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();

            var model = new TenantChangeViewModel
            {
                Tenant = loginInfo.Tenant,
            };

            return View(model);
        }
    }
}
