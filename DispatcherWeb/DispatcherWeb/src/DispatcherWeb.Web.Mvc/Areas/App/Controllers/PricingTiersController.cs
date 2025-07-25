using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.PricingTiers;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Items_PricingTiers)]
    public class PricingTiersController : DispatcherWebControllerBase
    {
        private readonly IPricingTierAppService _pricingTierAppService;

        public PricingTiersController(IPricingTierAppService pricingTierAppService)
        {
            _pricingTierAppService = pricingTierAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _pricingTierAppService.GetPricingTierForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }
    }
}
