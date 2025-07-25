using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Invoices;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Web.Areas.App.Models.Invoices;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class InvoicesController : DispatcherWebControllerBase
    {
        private readonly IInvoiceAppService _invoiceAppService;

        public InvoicesController(IInvoiceAppService invoiceAppService)
        {
            _invoiceAppService = invoiceAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public IActionResult Index(int? batchId)
        {
            return View(new InvoiceIndexViewModel
            {
                BatchId = batchId,
            });
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<IActionResult> Details(int? id)
        {
            var model = await _invoiceAppService.GetInvoiceForEdit(new NullableIdDto(id));
            return View(model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
        public PartialViewResult SelectCustomerTicketsModal(GetCustomerTicketsInput model)
        {
            return PartialView("_SelectCustomerTicketsModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
        public PartialViewResult SelectCustomerChargesModal(GetCustomerChargesInput model)
        {
            return PartialView("_SelectCustomerChargesModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<FileContentResult> GetInvoicePrintOut(GetInvoicePrintOutInput input)
        {
            var report = await _invoiceAppService.GetInvoicePrintOut(input);
            return InlinePdfFile(report.SaveToBytesArray(), "InvoicePrintOut.pdf");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Invoices, AppPermissions.Pages_Invoices_ApproveInvoices, RequireAllPermissions = true)]
        public async Task<IActionResult> PrintApprovedInvoices()
        {
            var report = await _invoiceAppService.PrintApprovedInvoices();
            if (report != null)
            {
                return InlinePdfFile(report.SaveToBytesArray(), "ApprovedInvoices.pdf");
            }
            else
            {
                return Ok(null);
            }
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Invoices, RequireAllPermissions = true)]
        public async Task<IActionResult> PrintDraftInvoices(PrintDraftInvoicesInput input)
        {
            var report = await _invoiceAppService.PrintDraftInvoices(input);
            if (report != null)
            {
                return InlinePdfFile(report.SaveToBytesArray(), "DraftInvoices.pdf");
            }
            else
            {
                return Ok(null);
            }
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PartialViewResult> EmailInvoicePrintOutModal(int id)
        {
            var model = await _invoiceAppService.GetEmailInvoicePrintOutModel(new EntityDto(id));
            return PartialView("_EmailInvoicePrintOutModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PartialViewResult> EmailOrPrintApprovedInvoicesModal()
        {
            var model = await _invoiceAppService.GetEmailOrPrintApprovedInvoicesModalModel();
            return PartialView("_EmailOrPrintApprovedInvoicesModal", model);
        }
    }
}
