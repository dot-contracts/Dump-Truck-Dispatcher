using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Reports_DriverActivityDetail)]
    public class DriverActivityDetailReportController : DispatcherWebControllerBase
    {
        private readonly IDispatchingAppService _dispatchingAppService;

        public DriverActivityDetailReportController(IDispatchingAppService dispatchingAppService)
        {
            _dispatchingAppService = dispatchingAppService;
        }

        public IActionResult Index()
        {
            var model = new ReportViewModelBase
            {
                ShowCsv = false,
            };
            return View(model);
        }

        public async Task<FileContentResult> GetReport(GetDriverActivityDetailReportInput input)
        {
            var report = await _dispatchingAppService.GetDriverActivityDetailReport(input);
            return InlinePdfFile(report.SaveToBytesArray(), "DriverActivityReport.pdf");
        }
    }
}