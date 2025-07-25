using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.JobSummary;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Orders_ViewJobSummary)]
    public class JobSummaryController : DispatcherWebControllerBase
    {
        private readonly IJobSummaryAppService _jobSummaryAppService;

        public JobSummaryController(
            IJobSummaryAppService jobSummaryAppService
            )
        {
            _jobSummaryAppService = jobSummaryAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Orders_ViewJobSummary)]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _jobSummaryAppService.GetJobSummaryHeaderDetails(id);
            return View(model);
        }
    }
}
