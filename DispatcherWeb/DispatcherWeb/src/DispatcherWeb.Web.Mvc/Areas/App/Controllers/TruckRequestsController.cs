using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.LeaseHaulerPortal_Truck_Request)]
    public class TruckRequestsController : DispatcherWebControllerBase
    {
        private readonly ILeaseHaulerRequestEditAppService _leaseHaulerRequestEdit;

        public TruckRequestsController(ILeaseHaulerRequestEditAppService leaseHaulerRequestEdit)
        {
            _leaseHaulerRequestEdit = leaseHaulerRequestEdit;
        }

        [AbpMvcAuthorize(AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public IActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<PartialViewResult> EditTruckRequestDetailModal(GetLeaseHaulerRequestForEditInput input)
        {
            var model = await _leaseHaulerRequestEdit.GetLeaseHaulerRequestForEdit(input);
            return PartialView("_EditTruckRequestDetailModal", model);
        }
    }
}
