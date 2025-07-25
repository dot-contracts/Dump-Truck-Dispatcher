using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_TimeEntry_EditTimeClassifications)]
    public class TimeClassificationsController : DispatcherWebControllerBase
    {
        private readonly ITimeClassificationAppService _timeClassificationAppService;

        public TimeClassificationsController(ITimeClassificationAppService timeClassificationAppService)
        {
            _timeClassificationAppService = timeClassificationAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _timeClassificationAppService.GetTimeClassificationForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }
    }
}