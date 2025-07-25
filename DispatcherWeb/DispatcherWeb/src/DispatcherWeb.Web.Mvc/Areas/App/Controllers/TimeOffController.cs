using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_TimeOff)]
    public class TimeOffController : DispatcherWebControllerBase
    {
        private readonly ITimeOffAppService _timeOffAppService;

        public TimeOffController(ITimeOffAppService timeOffAppService)
        {
            _timeOffAppService = timeOffAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _timeOffAppService.GetTimeOffForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> AddTimeEntryModal(int? id)
        {
            var model = await _timeOffAppService.GetTimeOffForEdit(new NullableIdDto(id));
            if (model.EmployeeId == null)
            {
                throw new UserFriendlyException(L("DriverIsNotLinkedToUser"));
            }
            return PartialView("_AddTimeEntryModal", model);
        }
    }
}