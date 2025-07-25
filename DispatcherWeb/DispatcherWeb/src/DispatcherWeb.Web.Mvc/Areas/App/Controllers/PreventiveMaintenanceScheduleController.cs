using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.PreventiveMaintenanceSchedule;
using DispatcherWeb.PreventiveMaintenanceSchedule.Dto;
using DispatcherWeb.Web.Areas.App.Models.PreventiveMaintenanceSchedule;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_View)]
    public class PreventiveMaintenanceScheduleController : DispatcherWebControllerBase
    {
        private readonly IPreventiveMaintenanceAppService _preventiveMaintenanceAppService;

        public PreventiveMaintenanceScheduleController(
            IPreventiveMaintenanceAppService preventiveMaintenanceAppService
        )
        {
            _preventiveMaintenanceAppService = preventiveMaintenanceAppService;
        }

        public IActionResult Index(int? filterStatus, bool? filterDueForService, int? filterOfficeId)
        {
            var model = new PreventiveMaintenanceListViewModel { Filter = new GetPreventiveMaintenancePagedListInput() };
            if (filterStatus.HasValue || filterDueForService.HasValue)
            {
                model.Filter.Status = filterStatus.HasValue ? (GetPreventiveMaintenancePagedListInput.PreventiveMaintenanceStatus)filterStatus.Value : GetPreventiveMaintenancePagedListInput.PreventiveMaintenanceStatus.All;
                model.Filter.DueForService = filterDueForService ?? false;
                model.Filter.OfficeId = filterOfficeId;
                model.DisableLoadState = true;
            }
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditPreventiveMaintenanceModal(int? id, int? truckId, string truckCode)
        {
            var model = await _preventiveMaintenanceAppService.GetForEdit(new NullableIdDto(id));
            if (id == null && truckId != null)
            {
                model.TruckId = truckId.Value;
                model.TruckCode = truckCode;
            }
            return PartialView("_CreateOrEditPreventiveMaintenanceModal", model);
        }

    }
}
