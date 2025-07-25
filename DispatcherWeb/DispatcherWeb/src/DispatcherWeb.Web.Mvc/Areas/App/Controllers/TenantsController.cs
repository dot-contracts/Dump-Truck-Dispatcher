using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common;
using DispatcherWeb.Editions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Web.Areas.App.Models.Tenants;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Tenants)]
    public class TenantsController : DispatcherWebControllerBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly ICommonLookupAppService _commonLookupAppService;
        private readonly TenantManager _tenantManager;
        private readonly IEditionAppService _editionAppService;

        public TenantsController(
            ITenantAppService tenantAppService,
            TenantManager tenantManager,
            IEditionAppService editionAppService,
            ICommonLookupAppService commonLookupAppService
            )
        {
            _tenantAppService = tenantAppService;
            _tenantManager = tenantManager;
            _editionAppService = editionAppService;
            _commonLookupAppService = commonLookupAppService;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.FilterText = Request.Query["filterText"];
            ViewBag.Sorting = Request.Query["sorting"];
            ViewBag.SubscriptionEndDateStart = Request.Query["subscriptionEndDateStart"];
            ViewBag.SubscriptionEndDateEnd = Request.Query["subscriptionEndDateEnd"];
            ViewBag.CreationDateStart = Request.Query["creationDateStart"];
            ViewBag.CreationDateEnd = Request.Query["creationDateEnd"];
            ViewBag.EditionId = Request.Query["editionId"];

            return View(new TenantIndexViewModel
            {
                EditionItems = await _editionAppService.GetEditionComboboxItems(null, true),
            });
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tenants_Create)]
        public async Task<PartialViewResult> CreateModal()
        {
            var editionItems = await _editionAppService.GetEditionComboboxItems();
            var defaultEditionName = (await _commonLookupAppService.GetDefaultEditionName()).Name;
            var defaultEditionItem = editionItems.FirstOrDefault(e => e.DisplayText == defaultEditionName);
            if (defaultEditionItem != null)
            {
                defaultEditionItem.IsSelected = true;
            }

            var viewModel = new CreateTenantViewModel(editionItems)
            {
            };

            return PartialView("_CreateModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task<PartialViewResult> EditModal(int id)
        {
            var tenantEditDto = await _tenantAppService.GetTenantForEdit(new EntityDto(id));
            var editionItems = await _editionAppService.GetEditionComboboxItems(tenantEditDto.EditionId);
            var viewModel = new EditTenantViewModel(tenantEditDto, editionItems);

            return PartialView("_EditModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task<PartialViewResult> FeaturesModal(int id)
        {
            var tenant = await (await _tenantManager.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                })
                .FirstAsync(x => x.Id == id);

            var tenantFeaturesModel = await _tenantAppService.GetTenantFeaturesForEdit(new EntityDto(id));
            var viewModel = new TenantFeaturesEditViewModel(tenantFeaturesModel)
            {
                TenantName = tenant.Name,
            };

            return PartialView("_FeaturesModal", viewModel);
        }
    }
}
