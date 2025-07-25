using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Trucks;
using DispatcherWeb.Web.Areas.App.Models.Dispatches;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Dispatches)]
    public class DispatchesController : DispatcherWebControllerBase
    {
        private readonly IDispatchingAppService _dispatchingAppService;
        private readonly ITruckAppService _truckAppService;

        public DispatchesController(
            IDispatchingAppService dispatchingAppService,
            ITruckAppService truckAppService
            )
        {
            _dispatchingAppService = dispatchingAppService;
            _truckAppService = truckAppService;
        }

        public async Task<IActionResult> Index(int? orderLineId, int? truckId)
        {
            if (!await SettingManager.DispatchViaAny())
            {
                return NotFound();
            }
            var model = new DispatchListViewModel
            {
                OrderLineId = orderLineId,
                TruckId = truckId,
                TruckCode = truckId.HasValue ? await _truckAppService.GetTruckCodeByTruckId(truckId.Value) : null,
            };
            return View(model);
        }

        public async Task<IActionResult> TruckDispatchList()
        {
            if (!await SettingManager.DispatchViaAny())
            {
                return NotFound();
            }
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> ViewDispatchModal(int id)
        {
            var model = await _dispatchingAppService.ViewDispatch(id);
            return PartialView("_ViewDispatchModal", model);
        }

        [Modal]
        public PartialViewResult DuplicateDispatchMessageModal(int id)
        {
            var model = new DuplicateDispatchInput() { DispatchId = id };
            return PartialView("_DuplicateDispatchMessageModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> SetDispatchTimeOnJobModal(int id)
        {
            var model = await _dispatchingAppService.GetDispatchTimeOnJob(id);
            return PartialView("_SetDispatchTimeOnJobModal", model);
        }

        [Modal]
        public PartialViewResult AddTicketPhotoModal()
        {
            return PartialView("_AddTicketPhotoModal");
        }
    }
}
