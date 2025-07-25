using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Debugging;
using DispatcherWeb.Editions;
using DispatcherWeb.Identity;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Security;
using DispatcherWeb.Url;
using DispatcherWeb.Web.Models.TenantRegistration;
using DispatcherWeb.Web.Security.Recaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.Controllers
{
    public class TenantRegistrationController : DispatcherWebControllerBase
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly UserManager _userManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly IWebUrlService _webUrlService;
        private readonly ITenantRegistrationAppService _tenantRegistrationAppService;
        private readonly IPasswordComplexitySettingStore _passwordComplexitySettingStore;
        private readonly IConfigurationRoot _appConfiguration;

        public TenantRegistrationController(
            IMultiTenancyConfig multiTenancyConfig,
            UserManager userManager,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            LogInManager logInManager,
            SignInManager signInManager,
            IWebUrlService webUrlService,
            ITenantRegistrationAppService tenantRegistrationAppService,
            IPasswordComplexitySettingStore passwordComplexitySettingStore,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _multiTenancyConfig = multiTenancyConfig;
            _userManager = userManager;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _webUrlService = webUrlService;
            _tenantRegistrationAppService = tenantRegistrationAppService;
            _passwordComplexitySettingStore = passwordComplexitySettingStore;
            _appConfiguration = configurationAccessor.Configuration;
        }


        [AllowAnonymous]
        public async Task<ActionResult> SelectEdition()
        {
            await CheckTenantRegistrationIsEnabled();

            var output = await _tenantRegistrationAppService.GetEditionsForSelect();
            if (output.EditionsWithFeatures.IsNullOrEmpty())
            {
                return RedirectToAction("Register", "TenantRegistration");
            }

            var editionName = _appConfiguration["App:TenantSelfRegistrationEditionName"];
            var edition = output.EditionsWithFeatures.FirstOrDefault(x => x.Edition.Name == editionName);
            if (edition != null)
            {
                return RedirectToAction("Register", "TenantRegistration", new
                {
                    editionId = edition.Edition.Id,
                    subscriptionStartType = SubscriptionStartType.Free,
                });
            }

            return RedirectToAction("Register", "TenantRegistration");

            //var model = new EditionsSelectViewModel(output);

            //return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Register(int? editionId, SubscriptionStartType? subscriptionStartType = null, SubscriptionPaymentGatewayType? gateway = null, string paymentId = "")
        {
            await CheckTenantRegistrationIsEnabled();

            var model = new TenantRegisterViewModel
            {
                PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync(),
                SubscriptionStartType = subscriptionStartType,
                EditionPaymentType = EditionPaymentType.NewRegistration,
                Gateway = gateway,
                PaymentId = paymentId,
            };

            if (editionId.HasValue)
            {
                model.EditionId = editionId.Value;
                model.Edition = await _tenantRegistrationAppService.GetEdition(editionId.Value);
            }

            ViewBag.UseCaptcha = UseCaptchaOnRegistration();

            return View(model);
        }

        [HttpPost]
        [UnitOfWork]
        public virtual async Task<ActionResult> Register(RegisterTenantInput model)
        {
            try
            {
                if (await UseCaptchaOnRegistration())
                {
                    var form = await Request.ReadFormAsync();
                    model.CaptchaResponse = form[RecaptchaValidator.RecaptchaResponseKey];
                }

                var result = await _tenantRegistrationAppService.RegisterTenant(model);

                CurrentUnitOfWork.SetTenantId(result.TenantId);

                var user = await _userManager.FindByNameAsync(AbpUserBase.AdminUserName);

                //Directly login if possible
                if (result.IsTenantActive && result.IsActive && !result.IsEmailConfirmationRequired
                    && !_webUrlService.SupportsTenancyNameInUrl)
                {
                    var loginResult = await GetLoginResultAsync(user.UserName, model.AdminPassword, result.TenancyName);

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(loginResult.Identity, false);

                        SetTenantIdCookie(result.TenantId);

                        return Redirect(Url.Action("Index", "Home", new { area = "App" }));
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " + loginResult.Result);
                }

                //Show result page
                var resultModel = new TenantRegisterResultViewModel
                {
                    TenantLoginAddress = _webUrlService.SupportsTenancyNameInUrl
                        ? _webUrlService.GetSiteRootAddress(result.TenancyName).EnsureEndsWith('/') + "Account/Login"
                        : "",
                    TenantId = result.TenantId,
                    TenancyName = result.TenancyName,
                    EmailAddress = result.EmailAddress,
                    IsTenantActive = result.IsTenantActive,
                    IsActive = result.IsActive,
                    IsEmailConfirmationRequired = result.IsEmailConfirmationRequired,
                };

                return View("RegisterResult", resultModel);
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.UseCaptcha = UseCaptchaOnRegistration();
                ViewBag.ErrorMessage = ex.Message;

                var viewModel = new TenantRegisterViewModel
                {
                    PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync(),
                    EditionId = model.EditionId,
                    SubscriptionStartType = model.SubscriptionStartType,
                    EditionPaymentType = EditionPaymentType.NewRegistration,
                    Gateway = model.Gateway,
                    PaymentId = model.PaymentId,
                };

                if (model.EditionId.HasValue)
                {
                    viewModel.Edition = await _tenantRegistrationAppService.GetEdition(model.EditionId.Value);
                    viewModel.EditionId = model.EditionId.Value;
                }

                return View("Register", viewModel);
            }
        }

        private async Task<bool> IsSelfRegistrationEnabled()
        {
            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.TenantManagement.AllowSelfRegistration);
        }

        private async Task CheckTenantRegistrationIsEnabled()
        {
            if (!await IsSelfRegistrationEnabled())
            {
                throw new UserFriendlyException(L("SelfTenantRegistrationIsDisabledMessage_Detail"));
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                throw new UserFriendlyException(L("MultiTenancyIsNotEnabled"));
            }
        }

        private async Task<bool> UseCaptchaOnRegistration()
        {
            if (DebugHelper.IsDebug)
            {
                return false;
            }

            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.TenantManagement.UseCaptchaOnRegistration);
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }
    }
}
