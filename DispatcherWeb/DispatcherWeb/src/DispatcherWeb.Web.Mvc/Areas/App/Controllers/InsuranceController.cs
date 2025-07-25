using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Insurances;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class InsuranceController : DispatcherWebControllerBase
    {
        private readonly IInsuranceAppService _insuranceService;

        public InsuranceController(
            IInsuranceAppService insuranceService
        )
        {
            _insuranceService = insuranceService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Insurance)]
        public async Task<IActionResult> GetInsurancePhoto(int id)
        {
            var insurancePhoto = await _insuranceService.GetInsurancePhoto(id);
            if (insurancePhoto?.FileBytes == null)
            {
                return NotFound();
            }
            if (insurancePhoto.FileName.ToLowerInvariant().EndsWith(".pdf"))
            {
                return InlinePdfFile(insurancePhoto.FileBytes, insurancePhoto.FileName);
            }
            Response.Headers["Content-Disposition"] = "inline; filename=" + insurancePhoto.FileName.SanitizeFilename();
            return File(insurancePhoto.FileBytes, "image/jpeg");
        }
    }
}
