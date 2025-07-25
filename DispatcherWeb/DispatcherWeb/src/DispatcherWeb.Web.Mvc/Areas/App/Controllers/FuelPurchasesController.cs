using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.FuelPurchases;
using DispatcherWeb.Web.Areas.App.Models.FuelPurchases;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_FuelPurchases_View)]
    public class FuelPurchasesController : DispatcherWebControllerBase
    {
        private readonly IFuelPurchaseAppService _fuelPurchaseAppService;

        public FuelPurchasesController(
            IFuelPurchaseAppService fuelPurchaseAppService
        )
        {
            _fuelPurchaseAppService = fuelPurchaseAppService;
        }

        [HttpGet]
        [Route("app/fuel")]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_FuelPurchases_Edit)]
        public async Task<PartialViewResult> CreateOrEditFuelPurchaseModal(int? id, int? officeId)
        {
            var dto = await _fuelPurchaseAppService.GetFuelPurchaseForEdit(new NullableIdDto(id));
            var model = CreateOrEditFuelPurchaseModalViewModel.CreateFromFuelPurchaseEditDto(dto);
            model.OfficeId = officeId;
            return PartialView("_CreateOrEditFuelPurchaseModal", model);
        }


    }
}
