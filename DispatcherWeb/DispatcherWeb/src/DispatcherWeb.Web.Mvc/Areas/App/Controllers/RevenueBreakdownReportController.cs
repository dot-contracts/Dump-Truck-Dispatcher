using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;


namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Reports_RevenueBreakdown)]
    public class RevenueBreakdownReportController : DispatcherWebControllerBase
    {
        public IActionResult Index()
        {
            var model = new ReportViewModelBase
            {
                ShowPdf = false,
            };
            return View(model);
        }

    }
}