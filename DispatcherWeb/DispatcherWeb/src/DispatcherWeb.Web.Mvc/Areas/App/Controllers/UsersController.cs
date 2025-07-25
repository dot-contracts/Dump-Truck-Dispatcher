using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Web.Areas.App.Models.Users;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users)]
    public class UsersController : DispatcherWebControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly UserManager _userManager;
        private readonly IRoleAppService _roleAppService;
        private readonly IPermissionAppService _permissionAppService;

        public UsersController(
            IUserAppService userAppService,
            UserManager userManager,
            IRoleAppService roleAppService,
            IPermissionAppService permissionAppService
            )
        {
            _userAppService = userAppService;
            _userManager = userManager;
            _roleAppService = roleAppService;
            _permissionAppService = permissionAppService;
        }

        public async Task<ActionResult> Index()
        {
            var roles = new List<ComboboxItemDto>();
            var permissions = (await _permissionAppService.GetAllPermissionsAsync())
                .Items
                .Select(p => new ComboboxItemDto(p.Name, new string('-', p.Level * 2) + " " + p.DisplayName))
                .ToList();

            if (await IsGrantedAsync(AppPermissions.Pages_Administration_Roles))
            {
                var getRolesOutput = await _roleAppService.GetRolesForDropdown();
                roles = getRolesOutput.Items.Select(r => new ComboboxItemDto(r.Id.ToString(), r.DisplayName)).ToList();
            }

            permissions.Insert(0, new ComboboxItemDto("", ""));

            var model = new UsersViewModel
            {
                FilterText = Request.Query["filterText"],
                Roles = roles,
                Permissions = permissions,
                OnlyLockedUsers = false,
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(long? id)
        {
            var output = await _userAppService.GetUserForEdit(new NullableIdDto<long> { Id = id });
            var viewModel = new CreateOrEditUserModalViewModel(output)
            {
                User = output.User,
                Roles = output.Roles,
                AllOrganizationUnits = output.AllOrganizationUnits,
                MemberedOrganizationUnits = output.MemberedOrganizationUnits,
            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task<PartialViewResult> PermissionsModal(long id)
        {
            var output = await _userAppService.GetUserPermissionsForEdit(new EntityDto<long>(id));
            var viewModel = new UserPermissionsEditViewModel(output);

            return PartialView("_PermissionsModal", viewModel);
        }

        public ActionResult LoginAttempts()
        {
            var loginResultTypes = Enum.GetNames(typeof(AbpLoginResultType))
                .Select(e => new ComboboxItemDto(e, L("AbpLoginResultType_" + e)))
                .ToList();

            return View("LoginAttempts", new UserLoginAttemptsViewModel
            {
                LoginAttemptResults = loginResultTypes,
            });
        }
    }
}
