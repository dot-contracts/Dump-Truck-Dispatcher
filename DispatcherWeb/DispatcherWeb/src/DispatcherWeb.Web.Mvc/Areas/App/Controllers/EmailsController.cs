using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Emailing;
using DispatcherWeb.Emailing.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class EmailsController : DispatcherWebControllerBase
    {
        private readonly IEmailAppService _emailAppService;
        private readonly bool _trackEmailOpen;

        public EmailsController(
            IAppConfigurationAccessor configurationAccessor,
            IEmailAppService emailAppService
        )
        {
            _emailAppService = emailAppService;
            _trackEmailOpen = configurationAccessor.Configuration["App:TrackEmailOpen"] == "true";
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken(Order = 1000)] //Order is a workaround for a bug: https://github.com/aspnet/Mvc/issues/6306
        public async Task<ActionResult> TrackEmailOpen(TrackEmailOpenInput input)
        {
            if (_trackEmailOpen)
            {
                await _emailAppService.TrackEmailOpen(input);
            }

            return File(Convert.FromBase64String("R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="), "image/gif");
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken(Order = 1000)] //Order is a workaround for a bug: https://github.com/aspnet/Mvc/issues/6306
        [HttpPost]
        public async Task<ActionResult> TrackEvents()
        {
            var request = await Request.GetRawBodyStringAsync();
            var inputList = JsonConvert.DeserializeObject<List<TrackEventInput>>(request);
            foreach (var input in inputList)
            {
                System.Diagnostics.Debug.WriteLine($"{input.Timestamp} {input.Event}: {input.Email} ({input.TrackableEmailId})");
            }
            await _emailAppService.TrackEvents(inputList);
            return Json("ok");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PartialViewResult> ViewEmailHistoryModal(GetEmailHistoryInput input)
        {
            var model = await _emailAppService.GetEmailHistoryInput(input);
            return PartialView("_ViewEmailHistoryModal", model);
        }
    }
}
