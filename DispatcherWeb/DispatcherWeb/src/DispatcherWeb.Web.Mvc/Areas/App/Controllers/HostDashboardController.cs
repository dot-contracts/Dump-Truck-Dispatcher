using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Areas.App.Models.HostDashboard;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
    public class HostDashboardController : DispatcherWebControllerBase
    {
        private const int DashboardOnLoadReportDayCount = 7;

        public ActionResult Index()
        {
            return View(new HostDashboardViewModel(DashboardOnLoadReportDayCount));
        }

    }
}