using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.VehicleServiceTypes;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleServiceTypes_View)]
    public class VehicleServiceTypesController : DispatcherWebControllerBase
    {
        private readonly IVehicleServiceTypeAppService _vehicleServiceTypeAppService;

        public VehicleServiceTypesController(
            IVehicleServiceTypeAppService vehicleServiceTypeAppService
        )
        {
            _vehicleServiceTypeAppService = vehicleServiceTypeAppService;
        }
        public async Task<IActionResult> Index()
        {
            var model = await _vehicleServiceTypeAppService.GetList();
            return View(model);
        }

    }
}
