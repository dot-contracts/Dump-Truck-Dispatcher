using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Locations;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Locations)]
    public class LocationsController : DispatcherWebControllerBase
    {
        private readonly ILocationAppService _locationAppService;

        public LocationsController(ILocationAppService locationAppService)
        {
            _locationAppService = locationAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditLocationModal(GetLocationForEditInput input)
        {
            var model = await _locationAppService.GetLocationForEdit(input);
            return PartialView("_CreateOrEditLocationModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditLocationContactModal(int? id, int? locationId)
        {
            var model = await _locationAppService.GetLocationContactForEdit(new NullableIdDto(id));

            if (model.LocationId == 0 && locationId != null)
            {
                model.LocationId = locationId.Value;
            }

            return PartialView("_CreateOrEditLocationContactModal", model);
        }
    }
}
