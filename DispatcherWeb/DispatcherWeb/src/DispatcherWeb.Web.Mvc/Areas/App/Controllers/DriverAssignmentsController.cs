using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_DriverAssignment)]
    public class DriverAssignmentsController : DispatcherWebControllerBase
    {
        private readonly IDriverAssignmentAppService _driverAssignmentService;

        public DriverAssignmentsController(IDriverAssignmentAppService driverAssignmentService)
        {
            _driverAssignmentService = driverAssignmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<FileContentResult> GetReport(GetDriverAssignmentsInput input)
        {
            var report = await _driverAssignmentService.GetDriverAssignmentReport(input);
            return InlinePdfFile(report, "DriverAssignmentReport.pdf");
        }

        [Modal]
        public PartialViewResult SelectTimeModal()
        {
            return PartialView("_SelectTimeModal");
        }
    }
}