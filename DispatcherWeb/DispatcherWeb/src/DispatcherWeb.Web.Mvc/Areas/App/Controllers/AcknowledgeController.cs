using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class AcknowledgeController : DispatcherWebControllerBase
    {
        private readonly IDispatchingAppService _dispatchingAppService;

        public AcknowledgeController(
            IDispatchingAppService dispatchingAppService
        )
        {
            _dispatchingAppService = dispatchingAppService;
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [Route("app/acknowledge/{id}")]
        public async Task<ActionResult> Index(int id, bool editTicket)
        {
            var driverInfoDto = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = id,
                EditTicket = editTicket,
            });

            switch (driverInfoDto)
            {
                case DriverInfoNotFoundDto _:
                    return View("../../../../Views/Error/Error404");

                case DispatchLoadInfoDto dispatchLoadInfoDto when dispatchLoadInfoDto.DispatchStatus == DispatchStatus.Created || dispatchLoadInfoDto.DispatchStatus == DispatchStatus.Sent:
                    return View("DispatchAcknowledge", dispatchLoadInfoDto);

                case DispatchLoadInfoDto dispatchLoadInfoDto:
                    return View("DispatchLoadInfo", dispatchLoadInfoDto);

                case DispatchDestinationInfoDto dispatchDestinationLoadInfoDto:
                    return View("DispatchDestinationInfo", dispatchDestinationLoadInfoDto);

                case DispatchInfoCompletedDto _:
                    return View("Completed");

                case DispatchInfoExpiredDto _:
                    return View("Expired");

                case DispatchInfoCanceledDto _:
                    return View("Canceled");

                case DispatchInfoErrorAndRedirect redirectInfoDto:
                    return View("ErrorWithRedirect", redirectInfoDto);

                default:
                    throw new ApplicationException("Unexpected");
            }
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [Route("app/acknowledge/completed")]
        public ActionResult Completed()
        {
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [Route("app/acknowledge/expired")]
        public ActionResult Expired()
        {
            return View();
        }

    }
}
