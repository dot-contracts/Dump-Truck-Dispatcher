using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverMessages;
using DispatcherWeb.Drivers;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Web.Areas.App.Models.DriverMessages;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_DriverMessages)]
    public class DriverMessagesController : DispatcherWebControllerBase
    {
        private readonly IDriverMessageAppService _driverMessageAppService;
        private readonly IDriverAppService _driverAppService;

        public DriverMessagesController(
            IDriverMessageAppService driverMessageAppService,
            IDriverAppService driverAppService
        )
        {
            _driverMessageAppService = driverMessageAppService;
            _driverAppService = driverAppService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> SendMessageModal(int? orderLineId, int? selectedDriverId)
        {
            var model = new SendMessageModalViewModel
            {
                OrderLineId = orderLineId,
                SelectedDriverId = selectedDriverId,
            };
            if (orderLineId.HasValue)
            {
                model.Drivers = await _driverAppService.GetDriversFromOrderLineSelectList(new GetDriversFromOrderLineSelectListInput() { OrderLineId = orderLineId.Value, MaxResultCount = 1000 });
            }
            return PartialView("_SendMessageModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> ViewMessageModal(int id)
        {
            var model = await _driverMessageAppService.GetForView(id);
            return PartialView("_ViewMessageModal", model);
        }

    }
}
