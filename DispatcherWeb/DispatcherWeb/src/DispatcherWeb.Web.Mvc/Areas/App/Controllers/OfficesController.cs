using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Offices;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Offices)]
    public class OfficesController : DispatcherWebControllerBase
    {
        private readonly IOfficeAppService _officeAppService;

        public OfficesController(IOfficeAppService officeAppService)
        {
            _officeAppService = officeAppService;
        }

        public async Task<IActionResult> Index()
        {
            bool canCreateOffice = (await FeatureChecker.GetValueAsync(AppFeatures.AllowMultiOfficeFeature)).To<bool>()
                                   || (await _officeAppService.GetOfficesNumber()) == 0;
            ViewBag.CanCreateOffice = canCreateOffice;
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _officeAppService.GetOfficeForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }

        [Modal]
        public PartialViewResult SelectOfficeModal()
        {
            return PartialView("_SelectOfficeModal");
        }
    }
}
