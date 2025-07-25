using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.PayStatements;
using DispatcherWeb.PayStatements.Dto;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class DriverPayStatementsController : DispatcherWebControllerBase
    {
        private readonly IPayStatementAppService _payStatementAppService;

        public DriverPayStatementsController(
            IPayStatementAppService payStatementAppService
        )
        {
            _payStatementAppService = payStatementAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _payStatementAppService.GetPayStatementForEdit(new EntityDto(id));
            return View(model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public PartialViewResult AddPayStatementModal()
        {
            return PartialView("_AddPayStatementModal");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task<FileContentResult> GetDriverPayStatementReport(GetDriverPayStatementReportInput input)
        {
            var report = await _payStatementAppService.GetDriverPayStatementReport(input);
            if (report.FileName.ToLower().EndsWith(".pdf"))
            {
                return InlinePdfFile(report.FileBytes, report.FileName);
            }
            Response.Headers["Content-Disposition"] = "filename=" + report.FileName.SanitizeFilename();
            return File(report.FileBytes, report.MimeType);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public async Task<FileContentResult> GetDriverPayStatementWarningReport(EntityDto input)
        {
            var report = await _payStatementAppService.GetDriverPayStatementWarningsReport(input);
            if (report.FileName.ToLower().EndsWith(".pdf"))
            {
                return InlinePdfFile(report.FileBytes, report.FileName);
            }
            Response.Headers["Content-Disposition"] = "filename=" + report.FileName.SanitizeFilename();
            return File(report.FileBytes, report.MimeType);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Backoffice_DriverPay)]
        public PartialViewResult PrintDriverPayStatementModal(GetDriverPayStatementReportInput input)
        {
            return PartialView("_PrintDriverPayStatementModal", input);
        }

    }
}
