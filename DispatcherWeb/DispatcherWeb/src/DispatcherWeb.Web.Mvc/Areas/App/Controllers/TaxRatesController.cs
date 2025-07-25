using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.TaxRates;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Items_TaxRates_Edit)]
    public class TaxRatesController : DispatcherWebControllerBase
    {
        private readonly ITaxRateAppService _taxRateAppService;

        public TaxRatesController(ITaxRateAppService taxRateAppService)
        {
            _taxRateAppService = taxRateAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _taxRateAppService.GetTaxRateForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }
    }
}
