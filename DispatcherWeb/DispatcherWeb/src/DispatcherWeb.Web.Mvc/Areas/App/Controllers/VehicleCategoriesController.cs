using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.VehicleCategories;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleCategories)]
    public class VehicleCategoriesController : DispatcherWebControllerBase
    {
        private readonly IVehicleCategoryAppService _vehicleCategoryAppService;

        public VehicleCategoriesController(IVehicleCategoryAppService vehicleCategoryAppService)
        {
            _vehicleCategoryAppService = vehicleCategoryAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditVehicleCategoryModal(NullableIdDto input)
        {
            var model = await _vehicleCategoryAppService.GetVehicleCategoryForEdit(input);
            return PartialView("_CreateOrEditVehicleCategoryModal", model);
        }
    }
}