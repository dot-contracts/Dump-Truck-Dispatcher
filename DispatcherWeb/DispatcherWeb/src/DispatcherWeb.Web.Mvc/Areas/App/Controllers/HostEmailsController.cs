using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.HostEmails;
using DispatcherWeb.HostEmails.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_HostEmails)]
    public class HostEmailsController : DispatcherWebControllerBase
    {
        private readonly IHostEmailAppService _hostEmailAppService;

        public HostEmailsController(
            IHostEmailAppService hostEmailAppService
        )
        {
            _hostEmailAppService = hostEmailAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public PartialViewResult SendHostEmailModal()
        {
            return PartialView("_SendHostEmailModal", new SendHostEmailInput());
        }

        [Modal]
        public async Task<PartialViewResult> ViewHostEmailModal(EntityDto input)
        {
            var model = await _hostEmailAppService.GetHostEmailForView(input);
            return PartialView("_ViewHostEmailModal", model);
        }

    }
}
