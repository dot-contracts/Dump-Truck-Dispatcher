using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Configuration;
using Abp.MultiTenancy;
using DispatcherWeb.Authorization.Users.Profile;
using DispatcherWeb.Configuration;
using DispatcherWeb.Timing;
using DispatcherWeb.Timing.Dto;
using DispatcherWeb.Web.Areas.App.Models.Profile;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class ProfileController : DispatcherWebControllerBase
    {
        private readonly IProfileAppService _profileAppService;
        private readonly ITimingAppService _timingAppService;
        private readonly ITenantCache _tenantCache;

        public ProfileController(
            IProfileAppService profileAppService,
            ITimingAppService timingAppService,
            ITenantCache tenantCache)
        {
            _profileAppService = profileAppService;
            _timingAppService = timingAppService;
            _tenantCache = tenantCache;
        }

        public async Task<PartialViewResult> MySettingsModal()
        {
            var output = await _profileAppService.GetCurrentUserProfileForEdit();
            var timezoneItems = await _timingAppService.GetTimezoneComboboxItems(new GetTimezoneComboboxItemsInput
            {
                DefaultTimezoneScope = SettingScopes.User,
                SelectedTimezoneId = output.Timezone,
            });


            var viewModel = new MySettingsViewModel(output)
            {
                TimezoneItems = timezoneItems,
                SmsVerificationEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SmsVerificationEnabled),
            };

            return PartialView("_MySettingsModal", viewModel);
        }

        public PartialViewResult ChangePictureModal(int userId)
        {
            return PartialView("_ChangePictureModal", userId);
        }

        public PartialViewResult ChangePasswordModal()
        {
            return PartialView("_ChangePasswordModal");
        }



        public PartialViewResult SmsVerificationModal()
        {
            return PartialView("_SmsVerificationModal");
        }


        public PartialViewResult LinkedAccountsModal()
        {
            return PartialView("_LinkedAccountsModal");
        }

        public async Task<PartialViewResult> LinkAccountModal()
        {
            ViewBag.TenancyName = await GetTenancyNameOrNull();
            return PartialView("_LinkAccountModal");
        }

        public PartialViewResult UploadSignaturePictureModal()
        {
            return PartialView("_UploadSignaturePictureModal");
        }

        private async Task<string> GetTenancyNameOrNull()
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (!tenantId.HasValue)
            {
                return null;
            }

            return (await _tenantCache.GetOrNullAsync(tenantId.Value))?.TenancyName;
        }
    }
}
