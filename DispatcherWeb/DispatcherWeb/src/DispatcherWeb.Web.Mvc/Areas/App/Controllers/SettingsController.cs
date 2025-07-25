using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Configuration.Tenants;
using DispatcherWeb.Timing;
using DispatcherWeb.Timing.Dto;
using DispatcherWeb.Web.Areas.App.Models.Settings;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Tenant_Settings)]
    public class SettingsController : DispatcherWebControllerBase
    {
        private readonly UserManager _userManager;
        private readonly ITenantSettingsAppService _tenantSettingsAppService;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ITimingAppService _timingAppService;
        //private readonly IQuickbooksOnlineAppService _quickbooksOnlineAppService;
        private readonly IConfigurationRoot _appConfiguration;

        public SettingsController(
            ITenantSettingsAppService tenantSettingsAppService,
            IMultiTenancyConfig multiTenancyConfig,
            ITimingAppService timingAppService,
            //IQuickbooksOnlineAppService quickbooksOnlineAppService,
            UserManager userManager,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _tenantSettingsAppService = tenantSettingsAppService;
            _multiTenancyConfig = multiTenancyConfig;
            _timingAppService = timingAppService;
            //_quickbooksOnlineAppService = quickbooksOnlineAppService;
            _userManager = userManager;
            _appConfiguration = configurationAccessor.Configuration;
        }

        public async Task<ActionResult> Index()
        {
            var output = await _tenantSettingsAppService.GetAllSettings();
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;

            var timezoneItems = await _timingAppService.GetTimezoneComboboxItems(new GetTimezoneComboboxItemsInput
            {
                DefaultTimezoneScope = SettingScopes.Tenant,
                SelectedTimezoneId = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, await AbpSession.GetTenantIdAsync()),
            });

            var user = await _userManager.GetUserAsync(await AbpSession.ToUserIdentifierAsync());

            ViewBag.CurrentUserEmail = user.EmailAddress;

            var model = new SettingsViewModel
            {
                Settings = output,
                TimezoneItems = timezoneItems,
            };

            return View(model);
        }

        [Modal]
        public PartialViewResult TestSmsNumberModal(string testPurpose)
        {
            var model = new TestSmsNumberModalViewModel() { CountryCode = "1", TestPurpose = testPurpose };
            return PartialView("_TestSmsNumberModal", model);
        }

        /*
        public async Task<IActionResult> QuickbooksOnline()
        {
            var integrationKind = (QuickbooksIntegrationKind)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.Quickbooks.IntegrationKind);
            var isConnected = await SettingManager.IsQuickbooksConnected();

            if ((int)integrationKind != (int)QuickbooksDeprecatedIntegrationKind.Online || !isConnected)
            {
                return Redirect("Index");
            }

            var model = new QuickbooksOnlineViewModel
            {
                IncomeAccountList = (await _quickbooksOnlineAppService.GetIncomeAccountSelectList()).Select(x => new SelectListItem(x.Name, x.Id)).ToList(),
                DefaultIncomeAccountId = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountId),
                DefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountName)
            };
            return View(model);
        }
        */

        public async Task<IActionResult> LinkDtdTrackerAccount()
        {
            var redirectUrl = _appConfiguration["DtdTracker:oAuthUrl"];
            if (!await _tenantSettingsAppService.CanLinkDtdTrackerAccount() || string.IsNullOrEmpty(redirectUrl))
            {
                return RedirectToAction("Index");
            }
            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> LinkDtdTrackerAccountCallback(string access_token)
        {
            await _tenantSettingsAppService.LinkDtdTrackerAccount(access_token);
            return RedirectToAction("Index");
        }
    }
}
