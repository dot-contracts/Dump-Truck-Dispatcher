using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerPerformance)]
    public class LeaseHaulerPerformanceController : DispatcherWebControllerBase
    {
        public LeaseHaulerPerformanceController()
        {
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
