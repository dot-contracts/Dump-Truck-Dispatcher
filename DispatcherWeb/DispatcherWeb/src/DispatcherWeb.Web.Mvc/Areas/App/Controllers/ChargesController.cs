using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Charges;
using DispatcherWeb.Charges.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Charges)]
    public class ChargesController : DispatcherWebControllerBase
    {
        private readonly IChargeAppService _chargeAppService;

        public ChargesController(IChargeAppService chargeAppService)
        {
            _chargeAppService = chargeAppService;
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Charges)]
        public async Task<PartialViewResult> EditChargesModal(GetChargeOrderLineDetailsInput input)
        {
            var model = await _chargeAppService.GetChargeOrderLineDetails(input);
            return PartialView("_EditChargesModal", model);
        }
    }
}
