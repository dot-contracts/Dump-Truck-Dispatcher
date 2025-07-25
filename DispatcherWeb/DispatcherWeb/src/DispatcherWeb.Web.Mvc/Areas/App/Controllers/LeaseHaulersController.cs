using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Web.Areas.App.Models.LeaseHaulers;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal)]
    public class LeaseHaulersController : DispatcherWebControllerBase
    {
        private readonly ILeaseHaulerAppService _leaseHaulerAppService;

        public LeaseHaulersController(ILeaseHaulerAppService leaseHaulerAppService)
        {
            _leaseHaulerAppService = leaseHaulerAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public IActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerModal(int? id)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditLeaseHaulerModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerContactModal(int? id, int? leaseHaulerId)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerContactForEdit(new NullableIdDto(id));

            if (model.LeaseHaulerId == 0 && leaseHaulerId != null)
            {
                model.LeaseHaulerId = leaseHaulerId.Value;
            }

            return PartialView("_CreateOrEditLeaseHaulerContactModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerTruckModal(GetLeaseHaulerTruckForEditInput input)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerTruckForEdit(input);

            return PartialView("_CreateOrEditLeaseHaulerTruckModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal)]
        public async Task<PartialViewResult> CreateOrEditLeaseHaulerDriverModal(int? id, int? leaseHaulerId)
        {
            var model = await _leaseHaulerAppService.GetLeaseHaulerDriverForEdit(new NullableIdDto(id));

            if (model.LeaseHaulerId == 0 && leaseHaulerId != null)
            {
                model.LeaseHaulerId = leaseHaulerId.Value;
            }

            return PartialView("_CreateOrEditLeaseHaulerDriverModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public PartialViewResult CallLeaseHaulerContactsModal(int id)
        {
            return PartialView("_CallLeaseHaulerContactsModal", id);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task<PartialViewResult> SendMessageModal(int leaseHaulerId, int? leaseHaulerContactId, LeaseHaulerMessageType messageType)
        {
            var model = new SendMessageModalViewModel
            {
                LeaseHaulerId = leaseHaulerId,
                MessageType = messageType,
                Contacts = await _leaseHaulerAppService.GetLeaseHaulerContactSelectList(leaseHaulerId, leaseHaulerContactId, messageType),
            };
            return PartialView("_SendMessageModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHauler)]
        public PartialViewResult SelectLeaseHaulerModal()
        {
            return PartialView("_SelectLeaseHaulerModal");
        }
    }
}
