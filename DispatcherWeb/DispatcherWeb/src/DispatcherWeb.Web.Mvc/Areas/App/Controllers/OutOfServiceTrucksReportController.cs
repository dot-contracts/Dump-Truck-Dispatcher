using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Reports_OutOfServiceTrucks)]
    public class OutOfServiceTrucksReportController : DispatcherWebControllerBase
    {
        public IActionResult Index()
        {
            return View(new ReportViewModelBase());
        }

    }
}
