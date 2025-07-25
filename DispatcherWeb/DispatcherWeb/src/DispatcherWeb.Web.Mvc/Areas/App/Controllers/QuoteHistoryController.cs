using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.QuoteHistory;
using DispatcherWeb.Web.Areas.App.Models.QuoteHistory;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Quotes_View)]
    public class QuoteHistoryController : DispatcherWebControllerBase
    {
        private readonly IQuoteHistoryAppService _quoteHistoryAppService;

        public QuoteHistoryController(IQuoteHistoryAppService quoteHistoryAppService)
        {
            _quoteHistoryAppService = quoteHistoryAppService;
        }

        public IActionResult Index(QuoteHistoryIndexViewModel model)
        {
            return View(model);
        }

        public async Task<PartialViewResult> ViewQuoteHistoryModal(EntityDto input)
        {
            var model = await _quoteHistoryAppService.GetQuoteHistoryDetails(input);
            return PartialView("_ViewQuoteHistoryModal", model);
        }
    }
}