using System.Threading.Tasks;
using DispatcherWeb.Web.Areas.App.Models.Layout;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.Views;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.AppFooter
{
    public class AppFooterViewComponent : DispatcherWebViewComponent
    {
        private readonly IPerRequestSessionCache _sessionCache;

        public AppFooterViewComponent(IPerRequestSessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footerModel = new FooterViewModel
            {
                LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync(),
            };

            return View(footerModel);
        }
    }
}
