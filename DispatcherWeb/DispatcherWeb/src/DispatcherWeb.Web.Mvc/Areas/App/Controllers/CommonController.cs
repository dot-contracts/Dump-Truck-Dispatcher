using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Permissions.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common.Modals;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class CommonController : DispatcherWebControllerBase
    {
        private readonly IPermissionAppService _permissionAppService;

        public CommonController(IPermissionAppService permissionAppService)
        {
            _permissionAppService = permissionAppService;
        }

        public PartialViewResult LookupModal(LookupModalViewModel model)
        {
            return PartialView("Modals/_LookupModal", model);
        }

        public PartialViewResult EntityTypeHistoryModal(EntityHistoryModalViewModel input)
        {
            return PartialView("Modals/_EntityTypeHistoryModal", new EntityHistoryModalViewModel
            {
                EntityTypeFullName = input.EntityTypeFullName,
                EntityTypeDescription = input.EntityTypeDescription,
                EntityId = input.EntityId,
            });
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Customers_Merge, AppPermissions.Pages_Items_Merge, AppPermissions.Pages_Locations_Merge)]
        public PartialViewResult MergeModal(MergeModalViewModel model)
        {
            return PartialView("Modals/_MergeModal", model);
        }
        public async Task<PartialViewResult> PermissionTreeModal(List<string> grantedPermissionNames = null)
        {
            var permissions = (await _permissionAppService.GetAllPermissionsAsync()).Items.ToList();

            var model = new PermissionTreeModalViewModel
            {
                Permissions = permissions.Select(x => new FlatPermissionDto
                {
                    ParentName = x.ParentName,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                }).OrderBy(p => p.DisplayName).ToList(),
                GrantedPermissionNames = grantedPermissionNames,
            };

            return PartialView("Modals/_PermissionTreeModal", model);
        }

        public PartialViewResult InactivityControllerNotifyModal()
        {
            return PartialView("Modals/_InactivityControllerNotifyModal");
        }
    }
}
