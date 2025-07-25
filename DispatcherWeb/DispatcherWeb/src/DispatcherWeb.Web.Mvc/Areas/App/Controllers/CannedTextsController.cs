using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.CannedTexts;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_CannedText)]
    public class CannedTextsController : DispatcherWebControllerBase
    {
        private readonly ICannedTextAppService _cannedTextAppService;

        public CannedTextsController(ICannedTextAppService cannedTextAppService)
        {
            _cannedTextAppService = cannedTextAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            var model = await _cannedTextAppService.GetCannedTextForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditModal", model);
        }
    }
}