using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class LeaseHaulerStatementsController : DispatcherWebControllerBase
    {
        public LeaseHaulerStatementsController()
        {
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public PartialViewResult AddLeaseHaulerStatementModal()
        {
            return PartialView("_AddLeaseHaulerStatementModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public PartialViewResult SpecifyExportOptionsModal()
        {
            return PartialView("_SpecifyExportOptionsModal");
        }
    }
}
