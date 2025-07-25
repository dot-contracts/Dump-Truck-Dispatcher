using System.Threading.Tasks;
using DispatcherWeb.Web.Session;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Views.Shared.Components.AccountLogo
{
    public class AccountLogoViewComponent : DispatcherWebViewComponent
    {
        private readonly IPerRequestSessionCache _sessionCache;

        public AccountLogoViewComponent(IPerRequestSessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
            return View(new AccountLogoViewModel(loginInfo));
        }
    }
}
