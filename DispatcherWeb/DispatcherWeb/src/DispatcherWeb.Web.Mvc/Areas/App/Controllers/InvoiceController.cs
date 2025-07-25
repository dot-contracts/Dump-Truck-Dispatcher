using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.MultiTenancy.Accounting;
using DispatcherWeb.Web.Areas.App.Models.Accounting;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    public class InvoiceController : DispatcherWebControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;

        public InvoiceController(IInvoiceAppService invoiceAppService)
        {
            _invoiceAppService = invoiceAppService;
        }


        [HttpGet]

        [AllowAnonymous]
        public async Task<ActionResult> Index(long paymentId)
        {
            var invoice = await _invoiceAppService.GetInvoiceInfo(new EntityDto<long>(paymentId));
            var model = new InvoiceViewModel
            {
                Invoice = invoice,
            };

            return View(model);
        }
    }
}
