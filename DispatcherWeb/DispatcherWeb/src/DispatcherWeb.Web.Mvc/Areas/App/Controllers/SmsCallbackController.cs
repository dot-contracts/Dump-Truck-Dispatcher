using System.Threading.Tasks;
using Abp.Domain.Uow;
using DispatcherWeb.Sms;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    public class SmsCallbackController : DispatcherWebControllerBase
    {
        private readonly ISmsAppService _smsAppService;

        public SmsCallbackController(
            ISmsAppService smsAppService
        )
        {
            _smsAppService = smsAppService;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken(Order = 1000)] //Order is a workaround for a bug: https://github.com/aspnet/Mvc/issues/6306
        [UnitOfWork(IsDisabled = true)]
        [HttpPost]
        public async Task<ActionResult> Index()
        {
            var form = await Request.ReadFormAsync();
            var smsSid = form["SmsSid"];
            var messageStatus = form["MessageStatus"];
            await _smsAppService.SetSmsStatus(smsSid, messageStatus);

            return Content("Handled");
        }
    }
}
