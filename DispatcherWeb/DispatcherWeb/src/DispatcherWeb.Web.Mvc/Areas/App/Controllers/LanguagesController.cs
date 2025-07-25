using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Localization;
using DispatcherWeb.Web.Areas.App.Models.Languages;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Languages)]
    public class LanguagesController : DispatcherWebControllerBase
    {
        private readonly ILanguageAppService _languageAppService;

        public LanguagesController(
            ILanguageAppService languageAppService)
        {
            _languageAppService = languageAppService;
        }

        public async Task<ActionResult> Index()
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            var viewModel = new LanguagesIndexViewModel
            {
                IsTenantView = tenantId.HasValue,
            };

            return View(viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Languages_Create, AppPermissions.Pages_Administration_Languages_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var output = await _languageAppService.GetLanguageForEdit(new NullableIdDto { Id = id });
            var viewModel = new CreateOrEditLanguageModalViewModel(output);

            return PartialView("_CreateOrEditModal", viewModel);
        }
    }
}
