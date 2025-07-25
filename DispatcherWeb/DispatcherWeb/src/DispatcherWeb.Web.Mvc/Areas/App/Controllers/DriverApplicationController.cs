using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_DriverApplication)]
    public class DriverApplicationController : DispatcherWebControllerBase
    {
        private readonly IAppConfigurationAccessor _configurationAccessor;

        public DriverApplicationController(
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _configurationAccessor = configurationAccessor;
        }

        public IActionResult PWA()
        {
            return Redirect(_configurationAccessor.Configuration["App:DriverApplicationUri"]);
        }

        public ActionResult Index()
        {
            //todo: use per-tenant setting instead, with a fallback to an application wide setting.
            if (_configurationAccessor.Configuration["App:DriverApplicationVersion"] == "2")
            {
                return Redirect(_configurationAccessor.Configuration["App:DriverApplicationUri"]);
            }
            else //if (_configurationAccessor.Configuration["App:DriverApplicationVersion"] == "3")
            {
                //todo add a custom view that will try to redirect to com.dumptruckdispatcher.driver://auth, but if it fails, then redirect to google play store page (or other store)
                return Redirect(_configurationAccessor.Configuration["App:DriverApplication3Uri"]);
            }
        }
    }
}
