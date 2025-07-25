using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.TrailerAssignments.Dto;
using DispatcherWeb.Web.Areas.App.Models.Scheduling;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
    public class SchedulingController : DispatcherWebControllerBase
    {
        private readonly ISchedulingAppService _schedulingAppService;
        private readonly IDispatchingAppService _dispatchingAppService;

        public SchedulingController(
            ISchedulingAppService schedulingAppService,
            IDispatchingAppService dispatchingAppService
            )
        {
            _schedulingAppService = schedulingAppService;
            _dispatchingAppService = dispatchingAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public PartialViewResult AssignTrucksModal(GetTrucksToAssignInput model)
        {
            return PartialView("_AssignTrucksModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SetTruckUtilizationModal(int id)
        {
            var model = await _schedulingAppService.GetOrderTruckUtilizationForEdit(new EntityDto(id));
            return PartialView("_SetTruckUtilizationModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult ChangeOrderLineUtilizationModal(int id)
        {
            return PartialView("_ChangeOrderLineUtilizationModal", new ChangeOrderLineUtilizationInput { OrderLineId = id });
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SetOrderOfficeIdModal(int id)
        {
            var model = await _schedulingAppService.GetOrderOfficeIdForEdit(new EntityDto(id));
            return PartialView("_SetOrderOfficeIdModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SetOrderDirectionsModal(int id)
        {
            var model = await _schedulingAppService.GetOrderDirectionsForEdit(new EntityDto(id));
            return PartialView("_SetOrderDirectionsModal", model);
        }


        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SetOrderLineNoteModal(int? id)
        {
            var model = await _schedulingAppService.GetOrderLineNoteForEdit(new NullableIdDto(id));
            return PartialView("_SetOrderLineNoteModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SendOrderLineToHaulingCompanyModal(int orderLineId)
        {
            var model = await _schedulingAppService.GetInputForSendOrderLineToHaulingCompany(orderLineId);
            return PartialView("_SendOrderLineToHaulingCompanyModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> SetOrderDateModal(int orderLineId)
        {
            var model = await _schedulingAppService.GetSetOrderDateInput(orderLineId);
            return PartialView("_SetOrderDateModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> ShowTruckOrdersModal(ShowTruckOrdersModalViewModel model)
        {
            model.StartTime = await _schedulingAppService.GetStartTimeForTruckOrderLines(new GetTruckOrdersInput
            {
                TruckId = model.TruckId,
                ScheduleDate = model.ScheduleDate,
                Shift = model.Shift,
            });

            return PartialView("_ShowTruckOrdersModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult TripsReportModal()
        {
            return PartialView("_TripsReportModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult CycleTimesModal()
        {
            return PartialView("_CycleTimesModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<PartialViewResult> SendDispatchMessageModal(int orderLineId, int? selectedOrderLineTruckId)
        {
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLineId);
            var canAddDispatchBasedOnTime = await _dispatchingAppService.CanAddDispatchBasedOnTime(new CanAddDispatchBasedOnTimeInput
            {
                OrderLineId = orderLineId,
                OrderLineTruckIds = dto.OrderLineTrucks.Select(x => x.OrderLineTruckId).ToList(),
            });

            return PartialView("_SendDispatchMessageModal",
                new SendDispatchMessageModalViewModel(dto, canAddDispatchBasedOnTime, selectedOrderLineTruckId));
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult AssignDriverForTruckModal(SetDriverForTruckInput model)
        {
            return PartialView("_AssignDriverForTruckModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> ActivateClosedTrucksModal(int orderLineId)
        {
            var trucks = await _schedulingAppService.GetClosedTrucksSelectList(orderLineId);
            var model = new ActivateClosedTrucksModalViewModel
            {
                Trucks = new SelectList(trucks, "Id", "Name"),
                OrderLineId = orderLineId,
            };
            return PartialView("_ActivateClosedTrucksModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> ReassignTrucksModal(int orderLineId)
        {
            var trucks = await _schedulingAppService.GetTrucksSelectList(orderLineId);
            var model = new ReassignTrucksModalViewModel
            {
                Trucks = trucks.Select(t => new SelectListItem(t.Name, t.Id, true)),
                OrderLineId = orderLineId,
            };
            return PartialView("_ReassignTrucksModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PartialViewResult> ChangeDriverForOrderLineTruckModal(int orderLineTruckId)
        {
            var model = await _schedulingAppService.GetOrderLineTruckToChangeDriverModel(orderLineTruckId);
            return PartialView("_ChangeDriverForOrderLineTruckModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult SelectTrailerModal(SelectTrailerModalViewModel model)
        {
            return PartialView("_SelectTrailerModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult SetTractorForTrailerModal(SetTractorForTrailerInput model)
        {
            return PartialView("_SetTractorForTrailerModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<IActionResult> ShowMap(int orderLineId)
        {
            string database = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Database);
            string mapBaseUrl = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.MapBaseUrl);
            mapBaseUrl = mapBaseUrl.EndsWith("/") ? mapBaseUrl : mapBaseUrl + "/";
            if (database.IsNullOrEmpty() || mapBaseUrl.IsNullOrEmpty())
            {
                throw new UserFriendlyException("The Database and/or Base Url in the Geotab Settings is empty!");
            }
            string deviceIdsString = await _schedulingAppService.GetDeviceIdsStringForOrderLineTrucks(orderLineId);
            string url = $"{mapBaseUrl}{database}/#map,liveVehicleIds:!({deviceIdsString})";
            return Redirect(url);
        }

    }
}
