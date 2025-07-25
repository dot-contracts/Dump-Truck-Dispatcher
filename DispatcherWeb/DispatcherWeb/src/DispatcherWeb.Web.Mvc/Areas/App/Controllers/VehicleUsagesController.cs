using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.VehicleUsages;
using DispatcherWeb.Web.Areas.App.Models.VehicleUsages;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleUsages_View)]
    public class VehicleUsagesController : DispatcherWebControllerBase
    {
        private readonly IVehicleUsageAppService _vehicleUsageAppService;

        public VehicleUsagesController(
            IVehicleUsageAppService vehicleUsageAppService
        )
        {
            _vehicleUsageAppService = vehicleUsageAppService;
        }

        [HttpGet]
        [Route("app/vehicleusages")]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_VehicleUsages_Edit)]
        public async Task<PartialViewResult> CreateOrEditVehicleUsageModal(int? id, int? officeId)
        {
            var dto = await _vehicleUsageAppService.GetVehicleUsageForEdit(new NullableIdDto(id));
            var model = CreateOrEditVehicleUsageModalViewModel.CreateFromVehicleUsageEditDto(dto);
            model.OfficeId = officeId;
            return PartialView("_CreateOrEditVehicleUsageModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        public IActionResult ImportVehicleUsageModal()
        {
            ViewBag.ImportType = ImportType.VehicleUsage;
            return PartialView("_ImportVehicleUsageModal");
        }
    }
}
