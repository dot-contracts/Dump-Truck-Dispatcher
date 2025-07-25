using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.HaulZones;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Items_HaulZones)]
    public class HaulZonesController : DispatcherWebControllerBase
    {
        private readonly IHaulZoneAppService _haulZoneAppService;

        public HaulZonesController(
            IHaulZoneAppService haulZoneAppService)
        {
            _haulZoneAppService = haulZoneAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(NullableIdNameDto input)
        {
            var model = await _haulZoneAppService.GetHaulZoneForEdit(input);
            return PartialView("_CreateOrEditModal", model);
        }
    }
}
