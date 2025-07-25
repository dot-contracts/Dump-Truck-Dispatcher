using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.EmployeeTime;
using DispatcherWeb.EmployeeTime.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_TimeEntry)]
    public class EmployeeTimeController : DispatcherWebControllerBase
    {
        private readonly IEmployeeTimeAppService _employeeTimeAppService;

        public EmployeeTimeController(IEmployeeTimeAppService employeeTimeAppService)
        {
            _employeeTimeAppService = employeeTimeAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _employeeTimeAppService.GetEmployeeTimeIndexModel();
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _employeeTimeAppService.GetEmployeeTimeForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_TimeEntry_EditAll)]
        public async Task<PartialViewResult> AddBulkTimeModal()
        {
            var timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var model = new AddBulkTimeDto
            {
                StartDateTime = Clock.Now.ConvertTimeZoneTo(timezone),
                EndDateTime = Clock.Now.ConvertTimeZoneTo(timezone),
            };

            return PartialView("_AddBulkTimeModal", model);
        }
    }
}
