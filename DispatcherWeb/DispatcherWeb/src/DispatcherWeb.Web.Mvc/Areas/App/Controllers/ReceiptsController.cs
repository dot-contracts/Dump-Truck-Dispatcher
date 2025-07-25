using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Receipts;
using DispatcherWeb.Receipts.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class ReceiptsController : DispatcherWebControllerBase
    {
        private readonly IReceiptAppService _receiptAppService;

        public ReceiptsController(
            IReceiptAppService receiptAppService
        )
        {
            _receiptAppService = receiptAppService;
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<IActionResult> Details(int? id, int? orderId)
        {
            var model = await _receiptAppService.GetReceiptForEdit(new GetReceiptForEditInput(id, orderId));
            return View(model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CreateOrEditReceiptLineModal(int? id, int? receiptId)
        {
            var model = await _receiptAppService.GetReceiptLineForEdit(new GetReceiptLineForEditInput(id, receiptId));
            return PartialView("_CreateOrEditReceiptLineModal", model);
        }

    }
}
