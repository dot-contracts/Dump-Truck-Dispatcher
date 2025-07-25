using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Quotes;
using DispatcherWeb.Quotes.Dto;
using DispatcherWeb.Web.Areas.App.Models.Quotes;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Quotes_View)]
    public class QuotesController : DispatcherWebControllerBase
    {
        private readonly IQuoteAppService _quoteAppService;

        public QuotesController(IQuoteAppService quoteAppService)
        {
            _quoteAppService = quoteAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int? id)
        {
            return Redirect("/app/quotes?id=" + id);
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditQuoteModal(int? id)
        {
            var model = await _quoteAppService.GetQuoteForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditQuoteModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditQuoteLineModal(GetQuoteLineForEditInput input)
        {
            var model = await _quoteAppService.GetQuoteLineForEdit(input);
            return PartialView("_CreateOrEditQuoteLineModal", model);
        }

        [Modal]
        public PartialViewResult ViewQuoteDeliveriesModal(ViewQuoteDeliveriesViewModel model)
        {
            return PartialView("_ViewQuoteDeliveriesModal", model);
        }

        public async Task<IActionResult> Copy(int id)
        {
            var newId = await _quoteAppService.CopyQuote(new EntityDto(id));
            return RedirectToAction("Details", new { id = newId });
        }

        public async Task<IActionResult> GetReport(GetQuoteReportInput input)
        {
            var report = await _quoteAppService.GetQuoteReport(input);
            return InlinePdfFile(report, "QuoteReport.pdf");
        }

        public async Task<PartialViewResult> EmailQuoteReportModal(int id)
        {
            var model = await _quoteAppService.GetEmailQuoteReportModel(new EntityDto(id));
            return PartialView("_EmailQuoteReportModal", model);
        }
    }
}
