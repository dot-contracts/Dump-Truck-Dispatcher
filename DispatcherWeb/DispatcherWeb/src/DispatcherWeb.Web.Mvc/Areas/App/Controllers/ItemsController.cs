using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.HaulingCategories;
using DispatcherWeb.Items;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Items)]
    public class ItemsController : DispatcherWebControllerBase
    {
        private readonly IItemAppService _serviceAppService;
        private readonly IProductLocationAppService _productLocationAppService;
        private readonly IHaulingCategoryAppService _haulingCategoryAppService;

        public ItemsController(
            IItemAppService serviceAppService,
            IProductLocationAppService productLocationAppService,
            IHaulingCategoryAppService haulingCategoryAppService)
        {
            _serviceAppService = serviceAppService;
            _productLocationAppService = productLocationAppService;
            _haulingCategoryAppService = haulingCategoryAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditItemModal(NullableIdNameDto input)
        {
            var model = await _serviceAppService.GetItemForEdit(input);
            return PartialView("_CreateOrEditItemModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditItemPriceModal(int? id, int? itemId)
        {
            var model = await _serviceAppService.GetItemPriceForEdit(new NullableIdDto(id));

            if (model.ItemId == 0 && itemId != null)
            {
                model.ItemId = itemId.Value;
            }

            return PartialView("_CreateOrEditItemPriceModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditRateModal(int? id, int? itemId)
        {
            var model = await _productLocationAppService.GetProductLocationForEdit(new NullableIdDto(id));

            if (model.ItemId == 0 && itemId != null)
            {
                model.ItemId = itemId.Value;
                model.ProductLocationPrices = await _productLocationAppService.GetEmptyProductLocationPrices();
            }

            return PartialView("_CreateOrEditRateModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditHaulingZoneRateModal(int? id, int? itemId)
        {
            var model = await _haulingCategoryAppService.GetHaulingCategoryForEdit(new NullableIdDto(id));

            if (model.ItemId == 0 && itemId != null)
            {
                model.ItemId = itemId.Value;
                model.HaulingCategoryPrices = await _haulingCategoryAppService.GetEmptyHaulingCategoryPrices();
            }

            return PartialView("_CreateOrEditHaulingZoneRateModal", model);
        }

    }
}
