using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.ScheduledReports;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
    public class ScheduledReportsController : DispatcherWebControllerBase
    {
        private readonly IScheduledReportAppService _scheduledReportAppService;

        public ScheduledReportsController(
            IScheduledReportAppService scheduledReportAppService
        )
        {
            _scheduledReportAppService = scheduledReportAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditScheduledReportModal(int? id)
        {
            var model = await _scheduledReportAppService.GetScheduledReportForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditScheduledReportModal", model);
        }
    }
}
