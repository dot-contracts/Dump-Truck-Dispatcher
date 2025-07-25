using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Organizations;
using DispatcherWeb.Web.Areas.App.Models.Common.Modals;
using DispatcherWeb.Web.Areas.App.Models.OrganizationUnits;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_OrganizationUnits)]
    public class OrganizationUnitsController : DispatcherWebControllerBase
    {
        private readonly OrganizationUnitAppService _organizationUnitAppService;

        public OrganizationUnitsController(OrganizationUnitAppService organizationUnitAppService)
        {
            _organizationUnitAppService = organizationUnitAppService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public PartialViewResult CreateModal(long? parentId)
        {
            return PartialView("_CreateModal", new CreateOrganizationUnitModalViewModel(parentId));
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task<PartialViewResult> EditModal(long id)
        {
            var organizationUnit = await _organizationUnitAppService.GetOrganizationUnitForEdit(id);

            return PartialView("_EditModal", organizationUnit);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers)]
        public PartialViewResult AddMemberModal(LookupModalViewModel model)
        {
            return PartialView("_AddMemberModal", model);
        }
    }
}
