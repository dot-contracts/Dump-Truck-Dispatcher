using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Web.Models;
using Abp.Zero.Configuration;
using DispatcherWeb.Authentication.TwoFactor.Google;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Accounts;
using DispatcherWeb.Authorization.Accounts.Dto;
using DispatcherWeb.Authorization.Impersonation;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Debugging;
using DispatcherWeb.Identity;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.Sessions;
using DispatcherWeb.Url;
using DispatcherWeb.Web.IdentityServer;
using DispatcherWeb.Web.Models.Account;
using DispatcherWeb.Web.Security.Recaptcha;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.Views.Shared.Components.TenantChange;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;

namespace DispatcherWeb.Web.Controllers
{
    public class AccountController : DispatcherWebControllerBase
    {
        private readonly UserManager _userManager;
        private readonly TenantManager _tenantManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IWebUrlService _webUrlService;
        private readonly IAppUrlService _appUrlService;
        private readonly IAppNotifier _appNotifier;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly IUserLinkManager _userLinkManager;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly IRecaptchaValidator _recaptchaValidator;
        private readonly IPerRequestSessionCache _sessionCache;
        private readonly ITenantCache _tenantCache;
        private readonly IAccountAppService _accountAppService;
        private readonly IUserCreatorService _userCreatorService;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IImpersonationManager _impersonationManager;
        private readonly ISmsSender _smsSender;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmailSender _emailSender;
        private readonly IdentityOptions _identityOptions;
        private readonly ISessionAppService _sessionAppService;
        private readonly IAbpStartupConfiguration _startupConfiguration;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<OneTimeLogin, Guid> _oneTimeLoginRepository;

        public AccountController(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            TenantManager tenantManager,
            IUnitOfWorkManager unitOfWorkManager,
            IAppNotifier appNotifier,
            IWebUrlService webUrlService,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            IUserLinkManager userLinkManager,
            LogInManager logInManager,
            SignInManager signInManager,
            IRecaptchaValidator recaptchaValidator,
            ITenantCache tenantCache,
            IAccountAppService accountAppService,
            IUserCreatorService userCreatorService,
            UserRegistrationManager userRegistrationManager,
            IImpersonationManager impersonationManager,
            IAppUrlService appUrlService,
            IPerRequestSessionCache sessionCache,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IOptions<IdentityOptions> identityOptions,
            IEncryptionService encryptionService,
            ISessionAppService sessionAppService,
            IAppConfigurationAccessor configurationAccessor,
            IAbpStartupConfiguration startupConfiguration,
            IRepository<OneTimeLogin, Guid> oneTimeLoginRepository)
        {
            _userManager = userManager;
            _multiTenancyConfig = multiTenancyConfig;
            _tenantManager = tenantManager;
            _unitOfWorkManager = unitOfWorkManager;
            _webUrlService = webUrlService;
            _appNotifier = appNotifier;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _userLinkManager = userLinkManager;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _recaptchaValidator = recaptchaValidator;
            _tenantCache = tenantCache;
            _accountAppService = accountAppService;
            _userCreatorService = userCreatorService;
            _userRegistrationManager = userRegistrationManager;
            _impersonationManager = impersonationManager;
            _appUrlService = appUrlService;
            _sessionCache = sessionCache;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _encryptionService = encryptionService;
            _identityOptions = identityOptions.Value;
            _sessionAppService = sessionAppService;
            _startupConfiguration = startupConfiguration;
            _appConfiguration = configurationAccessor.Configuration;
            _oneTimeLoginRepository = oneTimeLoginRepository;
        }

        #region Login / Logout

        [AllowAnonymous]
        public async Task<ActionResult> Login(string userNameOrEmailAddress = "", string returnUrl = "", string successMessage = "", string warningMessage = "", string ss = "", bool forceHostLogin = false)
        {
            if (IsInternetExplorer())
            {
                return RedirectToAction("NotSupported");
            }

            //todo add styles to LightLogin
            //if (returnUrl.Contains("driverapplicationclient"))
            //{
            //    ViewBag.ReturnUrl = returnUrl;
            //    ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            //    ViewBag.SingleSignIn = ss;
            //    return View("LightLogin");
            //}

            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (AbpSession.UserId.HasValue && tenantId.HasValue && !returnUrl.IsNullOrEmpty())
            {
                // Prevent user from going "back" after they logged in to the driver app
                if (returnUrl.Contains("driverapplicationclient"))
                {
                    var driverApplicationUri = _appConfiguration["App:DriverApplicationUri"];
                    if (!string.IsNullOrEmpty(driverApplicationUri))
                    {
                        return Redirect(driverApplicationUri);
                    }
                }

                return RedirectToAction("Index", "Error", new { statusCode = 403 });
            }
            var hideSelfRegistrationOption = !returnUrl.IsNullOrEmpty();
            returnUrl = NormalizeReturnUrl(returnUrl);

            if (!string.IsNullOrEmpty(ss)
                && ss.Equals("true", StringComparison.OrdinalIgnoreCase)
                && AbpSession.UserId > 0)
            {
                var updateUserSignInTokenOutput = await _sessionAppService.UpdateUserSignInToken();
                returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, updateUserSignInTokenOutput.SignInToken, AbpSession.UserId.Value, tenantId);
                return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            ViewBag.SingleSignIn = ss;
            //ViewBag.UseCaptcha = UseCaptchaOnLogin(); //todo

            await _signInManager.SignOutAsync();

            return View(
                new LoginFormViewModel
                {
                    IsSelfRegistrationEnabled = !hideSelfRegistrationOption && await IsSelfRegistrationEnabled(),
                    IsTenantSelfRegistrationEnabled = !hideSelfRegistrationOption && await IsTenantSelfRegistrationEnabled(),
                    IsEmailActivationEnabled = !hideSelfRegistrationOption,
                    SuccessMessage = successMessage,
                    WarningMessage = warningMessage,
                    UserNameOrEmailAddress = userNameOrEmailAddress,
                    ForceHostLogin = forceHostLogin,
                });
        }

        [AllowAnonymous]
        public IActionResult NotSupported()
        {
            return View();
        }

        private bool IsInternetExplorer()
        {
            string userAgent = Request.Headers["User-Agent"];
            return userAgent.Contains("MSIE ") || userAgent.Contains("Trident");
        }

        [HttpPost]
        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "", string ss = "")
        {
            var telemetry = new TelemetryClient();
            var startTime = DateTime.UtcNow;
            
            try
            {
                returnUrl = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrWhiteSpace(returnUrlHash))
                {
                    returnUrl += returnUrlHash;
                }

                var loginResult = await GetLoginResultAsync(loginModel.UsernameOrEmailAddress.Trim(), loginModel.Password, await GetTenancyNameOrNullAsync(), loginModel.ForceHostLogin);

                loginResult.User.LastLoginTime = Clock.Now;

                if (!string.IsNullOrEmpty(ss) && ss.Equals("true", StringComparison.OrdinalIgnoreCase) && loginResult.Result == AbpLoginResultType.Success)
                {
                    loginResult.User.SetSignInToken();
                    returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, loginResult.User.SignInToken, loginResult.User.Id, loginResult.User.TenantId);
                }

                if (loginResult.User.ShouldChangePasswordOnNextLogin)
                {
                    loginResult.User.SetNewPasswordResetCode();
                    await _userManager.UpdateAsync(loginResult.User);

                    telemetry.TrackMetric("Account_Login_PasswordChangeRequired", (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return Json(new AjaxResponse
                    {
                        TargetUrl = Url.Action(
                            "ResetPassword",
                            new ResetPasswordViewModel
                            {
                                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                                UserId = loginResult.User.Id,
                                ResetCode = loginResult.User.PasswordResetCode,
                                ReturnUrl = returnUrl,
                                SingleSignIn = ss,
                            }),
                    });
                }

                var signInResult = await _signInManager.SignInOrTwoFactorAsync(loginResult, loginModel.RememberMe);
                if (signInResult.RequiresTwoFactor)
                {
                    telemetry.TrackMetric("Account_Login_TwoFactorRequired", (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return Json(new AjaxResponse
                    {
                        TargetUrl = Url.Action(
                            "SendSecurityCode",
                            new
                            {
                                returnUrl = returnUrl,
                                rememberMe = loginModel.RememberMe,
                            }),
                    });
                }

                Debug.Assert(signInResult.Succeeded);

                await UnitOfWorkManager.Current.SaveChangesAsync();

                telemetry.TrackMetric("Account_Login_Success", (DateTime.UtcNow - startTime).TotalMilliseconds);
                return Json(new AjaxResponse { TargetUrl = returnUrl });
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                telemetry.TrackMetric("Account_Login_Error", (DateTime.UtcNow - startTime).TotalMilliseconds);
                throw;
            }
        }

        [HttpPost]
        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<IActionResult> LightLogin(LightLoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "", string ss = "")
        {
            if (string.IsNullOrEmpty(loginModel.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
            }

            if (!ModelState.IsValid)
            {
                UpdateViewBag();
                return View("LightLogin", loginModel);
            }

            returnUrl = NormalizeReturnUrl(returnUrl);
            if (!string.IsNullOrWhiteSpace(returnUrlHash))
            {
                returnUrl += returnUrlHash;
            }
            try
            {
                await SwitchToTenantIfNeeded(loginModel.TenancyName);
                var loginResult = await GetLoginResultAsync(loginModel.UsernameOrEmailAddress, loginModel.Password, loginModel.TenancyName);
                if (!string.IsNullOrEmpty(ss) && ss.Equals("true", StringComparison.OrdinalIgnoreCase) && loginResult.Result == AbpLoginResultType.Success)
                {
                    loginResult.User.SetSignInToken();
                    returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, loginResult.User.SignInToken, loginResult.User.Id, loginResult.User.TenantId);
                }

                //if (loginResult.User.ShouldChangePasswordOnNextLogin)
                //{
                //    loginResult.User.SetNewPasswordResetCode();

                //    return Json(new AjaxResponse
                //    {
                //        TargetUrl = Url.Action(
                //            "ResetPassword",
                //            new ResetPasswordViewModel
                //            {
                //                TenantId = AbpSession.TenantId,
                //                UserId = loginResult.User.Id,
                //                ResetCode = loginResult.User.PasswordResetCode,
                //                ReturnUrl = returnUrl,
                //                SingleSignIn = ss
                //            })
                //    });
                //}

                var signInResult = await _signInManager.SignInOrTwoFactorAsync(loginResult, loginModel.RememberMe);
                //if (signInResult.RequiresTwoFactor)
                //{
                //    return Json(new AjaxResponse
                //    {
                //        TargetUrl = Url.Action(
                //            "SendSecurityCode",
                //            new
                //            {
                //                returnUrl = returnUrl,
                //                rememberMe = loginModel.RememberMe
                //            })
                //    });
                //}

                Debug.Assert(signInResult.Succeeded);

                await UnitOfWorkManager.Current.SaveChangesAsync();

                //return Json(new AjaxResponse { TargetUrl = returnUrl });
                return Redirect(returnUrl);
            }
            catch (UserFriendlyException e)
            {
                ModelState.AddModelError("", e.Message + " - " + e.Details);
                UpdateViewBag();
                return View("LightLogin", loginModel);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                ModelState.AddModelError("", "An error occurred");
                UpdateViewBag();
                return View("LightLogin", loginModel);
            }

            void UpdateViewBag()
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
                ViewBag.SingleSignIn = ss;
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Logout([FromServices] IIdentityServerInteractionService interaction, string returnUrl = "", string logoutId = "")
        {
            if (await AbpSession.GetTenantIdOrNullAsync() == null)
            {
                _startupConfiguration.MultiTenancy.IsEnabled = _appConfiguration.IsMultitenancyEnabled();
            }
            await _signInManager.SignOutAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Logger.Info("Were asked to redirect to returnUrl: " + returnUrl);
                returnUrl = NormalizeReturnUrl(returnUrl);
                Logger.Info("Redirecting to returnUrl: " + returnUrl);
                return Redirect(returnUrl);
            }

            if (!string.IsNullOrEmpty(logoutId))
            {
                Logger.Info("LogoutId: " + logoutId);
                var logoutContext = await interaction.GetLogoutContextAsync(logoutId);
                Logger.Info("PostLogoutRedirectUri: " + logoutContext.PostLogoutRedirectUri);
                Logger.Info("logoutContext: " + JsonConvert.SerializeObject(logoutContext));
                if (!string.IsNullOrEmpty(logoutContext.PostLogoutRedirectUri))
                {
                    return Redirect(logoutContext.PostLogoutRedirectUri);
                }
            }

            return RedirectToAction("Login");
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName, bool forceHostLogin = false)
        {
            if (forceHostLogin)
            {
                tenancyName = null;
                SetTenantIdCookie(null);
                CurrentUnitOfWork.SetTenantId(null);
            }

            var loginResult = forceHostLogin
                ? await _logInManager.ForceHostLoginAsync(usernameOrEmailAddress, password)
                : await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string AddSingleSignInParametersToReturnUrl(string returnUrl, string signInToken, long userId, int? tenantId)
        {
            returnUrl += (returnUrl.Contains("?") ? "&" : "?") +
                         "accessToken=" + signInToken +
                         "&userId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString()));
            if (tenantId.HasValue)
            {
                returnUrl += "&tenantId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(tenantId.Value.ToString()));
            }

            return returnUrl;
        }

        #endregion

        #region Two Factor Auth

        [AllowAnonymous]
        public async Task<ActionResult> SendSecurityCode(string returnUrl, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            await CheckCurrentTenantAsync(await _signInManager.GetVerifiedTenantIdAsync());

            var userProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);

            var factorOptions = userProviders.Select(
                userProvider =>
                    new SelectListItem
                    {
                        Text = userProvider,
                        Value = userProvider,
                    }).ToList();

            return View(
                new SendSecurityCodeViewModel
                {
                    Providers = factorOptions,
                    ReturnUrl = returnUrl,
                    RememberMe = rememberMe,
                }
            );
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SendSecurityCode(SendSecurityCodeViewModel model)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            await CheckCurrentTenantAsync(await _signInManager.GetVerifiedTenantIdAsync());

            if (model.SelectedProvider != GoogleAuthenticatorProvider.Name)
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
                var message = L("EmailSecurityCodeBody", code);

                if (model.SelectedProvider == "Email")
                {
                    await _emailSender.SendAsync(await _userManager.GetEmailAsync(user), L("EmailSecurityCodeSubject"), message);
                }
                else if (model.SelectedProvider == "Phone")
                {
                    await _smsSender.SendAsync(new SmsSendInput
                    {
                        ToPhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                        Body = message,
                    });
                }
            }

            return RedirectToAction(
                "VerifySecurityCode",
                new
                {
                    provider = model.SelectedProvider,
                    returnUrl = model.ReturnUrl,
                    rememberMe = model.RememberMe,
                }
            );
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifySecurityCode(string provider, string returnUrl, bool rememberMe)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new UserFriendlyException(L("VerifySecurityCodeNotLoggedInErrorMessage"));
            }

            await CheckCurrentTenantAsync(await _signInManager.GetVerifiedTenantIdAsync());

            var isRememberBrowserEnabled = await IsRememberBrowserEnabledAsync();

            return View(
                new VerifySecurityCodeViewModel
                {
                    Provider = provider,
                    ReturnUrl = returnUrl,
                    RememberMe = rememberMe,
                    IsRememberBrowserEnabled = isRememberBrowserEnabled,
                }
            );
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> VerifySecurityCode(VerifySecurityCodeViewModel model)
        {
            model.ReturnUrl = NormalizeReturnUrl(model.ReturnUrl);

            await CheckCurrentTenantAsync(await _signInManager.GetVerifiedTenantIdAsync());

            var result = await _signInManager.TwoFactorSignInAsync(
                model.Provider,
                model.Code,
                model.RememberMe,
                await IsRememberBrowserEnabledAsync() && model.RememberBrowser
            );

            if (result.Succeeded)
            {
                return Json(new AjaxResponse { TargetUrl = model.ReturnUrl });
            }

            if (result.IsLockedOut)
            {
                throw new UserFriendlyException(L("UserLockedOutMessage"));
            }

            throw new UserFriendlyException(L("InvalidSecurityCode"));
        }

        private Task<bool> IsRememberBrowserEnabledAsync()
        {
            return SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled);
        }

        #endregion

        #region Register

        [AllowAnonymous]
        public Task<ActionResult> Register(string returnUrl = "", string ss = "")
        {
            return RegisterView(new RegisterViewModel
            {
                ReturnUrl = returnUrl,
                SingleSignIn = ss,
            });
        }

        private async Task<ActionResult> RegisterView(RegisterViewModel model)
        {
            await CheckSelfRegistrationIsEnabled();

            ViewBag.UseCaptcha = !model.IsExternalLogin && await UseCaptchaOnRegistration();

            return View("Register", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [UnitOfWork(IsolationLevel.ReadUncommitted)]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!model.IsExternalLogin && await UseCaptchaOnRegistration())
                {
                    var form = await Request.ReadFormAsync();
                    await _recaptchaValidator.ValidateAsync(form[RecaptchaValidator.RecaptchaResponseKey]);
                }

                ExternalLoginInfo externalLoginInfo = null;
                if (model.IsExternalLogin)
                {
                    externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
                    if (externalLoginInfo == null)
                    {
                        throw new Exception("Can not external login!");
                    }

                    model.UserName = model.EmailAddress.ToMd5();
                    model.Password = Authorization.Users.User.CreateRandomPassword();
                }
                else
                {
                    if (model.UserName.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
                    {
                        throw new UserFriendlyException(L("FormIsNotValidMessage"));
                    }
                }

                var user = await _userRegistrationManager.RegisterAsync(
                    model.Name,
                    model.Surname,
                    model.EmailAddress,
                    model.UserName,
                    model.Password,
                    false,
                    await _appUrlService.CreateEmailActivationUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync())
                );

                //Getting tenant-specific settings
                var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

                if (model.IsExternalLogin)
                {
                    Debug.Assert(externalLoginInfo != null);

                    if (string.Equals(externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email), model.EmailAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        user.IsEmailConfirmed = true;
                    }

                    user.Logins = new List<UserLogin>
                    {
                        new UserLogin
                        {
                            LoginProvider = externalLoginInfo.LoginProvider,
                            ProviderKey = externalLoginInfo.ProviderKey,
                            TenantId = user.TenantId,
                        },
                    };
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();

                Debug.Assert(user.TenantId != null);

                var tenant = await _tenantManager.GetByIdAsync(user.TenantId.Value);

                //Directly login if possible
                if (user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin))
                {
                    AbpLoginResult<Tenant, User> loginResult;
                    if (externalLoginInfo != null)
                    {
                        loginResult = await _logInManager.LoginAsync(externalLoginInfo, tenant.TenancyName);
                    }
                    else
                    {
                        loginResult = await GetLoginResultAsync(user.UserName, model.Password, tenant.TenancyName);
                    }

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await _signInManager.SignInAsync(loginResult.Identity, false);
                        if (!string.IsNullOrEmpty(model.SingleSignIn) && model.SingleSignIn.Equals("true", StringComparison.OrdinalIgnoreCase) && loginResult.Result == AbpLoginResultType.Success)
                        {
                            var returnUrl = NormalizeReturnUrl(model.ReturnUrl);
                            loginResult.User.SetSignInToken();
                            returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, loginResult.User.SignInToken, loginResult.User.Id, loginResult.User.TenantId);
                            return Redirect(returnUrl);
                        }

                        return Redirect(GetAppHomeUrl());
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " + loginResult.Result);
                }

                return View("RegisterResult", new RegisterResultViewModel
                {
                    TenancyName = tenant.TenancyName,
                    NameAndSurname = user.Name + " " + user.Surname,
                    UserName = user.UserName,
                    EmailAddress = user.EmailAddress,
                    IsActive = user.IsActive,
                    IsEmailConfirmationRequired = isEmailConfirmationRequiredForLogin,
                });
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.UseCaptcha = !model.IsExternalLogin && await UseCaptchaOnRegistration();
                ViewBag.ErrorMessage = ex.Message;

                return View("Register", model);
            }
        }

        private async Task<bool> UseCaptchaOnRegistration()
        {
            if (DebugHelper.IsDebug)
            {
                return false;
            }

            if (!(await AbpSession.GetTenantIdOrNullAsync()).HasValue)
            {
                //Host users can not register
                throw new InvalidOperationException();
            }

            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration);
        }

        private async Task CheckSelfRegistrationIsEnabled()
        {
            if (!await IsSelfRegistrationEnabled())
            {
                throw new UserFriendlyException(L("SelfUserRegistrationIsDisabledMessage_Detail"));
            }
        }

        private async Task<bool> IsSelfRegistrationEnabled()
        {
            if (!(await AbpSession.GetTenantIdOrNullAsync()).HasValue)
            {
                return false; //No registration enabled for host users!
            }

            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration);
        }

        private async Task<bool> IsTenantSelfRegistrationEnabled()
        {
            if ((await AbpSession.GetTenantIdOrNullAsync()).HasValue)
            {
                return false;
            }

            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.AllowSelfRegistration);
        }

        #endregion

        #region ForgotPassword / ResetPassword

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> SendPasswordResetLink(SendPasswordResetLinkViewModel model)
        {
            await _accountAppService.SendPasswordResetCode(
                new SendPasswordResetCodeInput
                {
                    EmailAddress = model.EmailAddress,
                });

            return Json(new AjaxResponse());
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            string errorMessage;
            try
            {
                model.ResolveParameters(_encryptionService);
                if (model.UserId == 0 && !string.IsNullOrEmpty(model.C))
                {
                    //the new decrypt method doesn't always throw a CryptographicException and sometimes just returns an empty string
                    throw new UserFriendlyException(L("CryptographicExceptionDuringLinkDecryption"));
                }

                await SwitchToTenantIfNeeded(model.TenantId);

                var user = await _userManager.GetUserByIdAsync(model.UserId);
                if (user == null || user.PasswordResetCode.IsNullOrEmpty() || user.PasswordResetCode != model.ResetCode)
                {
                    throw new UserFriendlyException(L("InvalidPasswordResetCode"), L("InvalidPasswordResetCode_Detail"));
                }

                return View(model);
            }
            catch (UserFriendlyException e)
            {
                errorMessage = $"{e.Message?.EnsureEndsWith('.')} {e.Details}";
            }
            catch (CryptographicException e)
            {
                Logger.Warn("Cryptographic exception during ResetPassword", e);
                errorMessage = L("CryptographicExceptionDuringLinkDecryption");
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected error during ResetPassword", e);
                errorMessage = L("UnexpectedErrorPleaseTryAgain");
            }

            return RedirectToAction("Login", new
            {
                warningMessage = errorMessage,
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordInput input)
        {
            var output = await _accountAppService.ResetPassword(input);

            if (output.CanLogin)
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                await _signInManager.SignInAsync(user, false);

                if (!string.IsNullOrEmpty(input.SingleSignIn) && input.SingleSignIn.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    user.SetSignInToken();
                    var returnUrl = AddSingleSignInParametersToReturnUrl(input.ReturnUrl, user.SignInToken, user.Id, user.TenantId);
                    return Redirect(returnUrl);
                }
            }

            return Redirect(NormalizeReturnUrl(input.ReturnUrl));
        }

        #endregion

        #region Email activation / confirmation

        [AllowAnonymous]
        public ActionResult EmailActivation()
        {
            return View();
        }

        [HttpPost]
        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<JsonResult> SendEmailActivationLink(SendEmailActivationLinkInput model)
        {
            await _accountAppService.SendEmailActivationLink(model);
            return Json(new AjaxResponse());
        }

        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<ActionResult> EmailConfirmation(EmailConfirmationViewModel input)
        {
            string errorMessage;
            try
            {
                input.ResolveParameters(_encryptionService);
                if (input.UserId == 0 && !string.IsNullOrEmpty(input.C))
                {
                    //the new decrypt method doesn't always throw a CryptographicException and sometimes just returns an empty string
                    throw new UserFriendlyException(L("CryptographicExceptionDuringLinkDecryption"));
                }

                await SwitchToTenantIfNeeded(input.TenantId);

                var user = await _userManager.GetUserByIdAsync(input.UserId);
                var isEmailConfirmed = user.IsEmailConfirmed;

                if (!isEmailConfirmed)
                {
                    await _accountAppService.ActivateEmail(input);
                }

                return RedirectToAction("Login", new
                {
                    successMessage = isEmailConfirmed ? L("YourEmailIsAlreadyConfirmed") : L("YourEmailIsConfirmedMessage"),
                    userNameOrEmailAddress = user.UserName,
                });
            }
            catch (UserFriendlyException e)
            {
                errorMessage = $"{e.Message?.EnsureEndsWith('.')} {e.Details}";
            }
            catch (CryptographicException e)
            {
                Logger.Warn("Cryptographic exception during EmailConfirmation", e);
                errorMessage = L("CryptographicExceptionDuringLinkDecryption");
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected error during EmailConfirmation", e);
                errorMessage = L("UnexpectedErrorPleaseTryAgain");
            }

            return RedirectToAction("Login", new
            {
                warningMessage = errorMessage,
            });
        }

        #endregion

        [AllowAnonymous]
        public virtual async Task<ActionResult> LoginByLink(Guid id)
        {
            await _signInManager.SignOutAsync();

            var oneTimeLogin = await (await _oneTimeLoginRepository.GetQueryAsync())
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (oneTimeLogin == null)
            {
                return RedirectToAction("Login");
            }

            if (oneTimeLogin.IsExpired || oneTimeLogin.ExpiryTime <= Clock.Now)
            {
                return RedirectToAction("Login", new { warningMessage = "Your link has expired." });
            }

            oneTimeLogin.IsExpired = true;

            var remainingOneTimeLogins = await (await _oneTimeLoginRepository.GetQueryAsync())
                .Where(x => x.UserId == oneTimeLogin.UserId && !x.IsExpired && x.Id != oneTimeLogin.Id)
                .ToListAsync();

            foreach (var remainingOneTimeLogin in remainingOneTimeLogins)
            {
                remainingOneTimeLogin.IsExpired = true;
            }

            User user;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                user = await (await _userManager.GetQueryAsync()).FirstOrDefaultAsync(x => x.Id == oneTimeLogin.UserId);
            }

            if (user == null)
            {
                return RedirectToAction("Login", new { warningMessage = "The user has been deleted" });
            }

            if (!user.IsActive)
            {
                return RedirectToAction("Login", new { warningMessage = "The user has been inactivated" });
            }

            var returnUrl = GetAppHomeUrl();

            if (!user.TenantId.HasValue)
            {
                Logger.Error($"LoginByLink doesn't support host users for security reasons (id {id}, userId {user.Id})");
                return RedirectToAction("Login");
            }

            var tenant = await _tenantManager.GetByIdAsync(user.TenantId.Value);

            await SwitchToTenantIfNeeded(tenant.TenancyName);

            user.IsEmailConfirmed = true;
            var tempPassword = await _userCreatorService.ResetUserPassword(user);
            await CurrentUnitOfWork.SaveChangesAsync();

            var loginResult = await GetLoginResultAsync(user.UserName, tempPassword, tenant.TenancyName);
            loginResult.User.LastLoginTime = Clock.Now;

            loginResult.User.SetNewPasswordResetCode();

            return Redirect(Url.Action(
                "ResetPassword",
                new ResetPasswordViewModel
                {
                    TenantId = loginResult.User.TenantId,
                    UserId = loginResult.User.Id,
                    ResetCode = loginResult.User.PasswordResetCode,
                    ReturnUrl = returnUrl,
                    SingleSignIn = "",
                })
            );
        }

        [AllowAnonymous]
        public async Task<ActionResult> SchedulingForTenant(int id)
        {
            await SwitchToTenantIfNeeded((await _tenantCache.GetOrNullAsync(id))?.TenancyName);
            return RedirectToAction("Index", "Scheduling", new { Area = "App" });
        }

        #region External Login

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl, string ss = "")
        {
            var redirectUrl = Url.Action(
                "ExternalLoginCallback",
                "Account",
                new
                {
                    ReturnUrl = returnUrl,
                    authSchema = provider,
                    ss = ss,
                });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl, string remoteError = null, string ss = "")
        {
            returnUrl = NormalizeReturnUrl(returnUrl);

            if (remoteError != null)
            {
                Logger.Error("Remote Error in ExternalLoginCallback: " + remoteError);
                throw new UserFriendlyException(L("CouldNotCompleteLoginOperation"));
            }

            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                Logger.Warn("Could not get information from external login.");
                return RedirectToAction(nameof(Login));
            }

            var tenancyName = await GetTenancyNameOrNullAsync();

            var loginResult = await _logInManager.LoginAsync(externalLoginInfo, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        await _signInManager.SignInAsync(loginResult.Identity, false);

                        if (!string.IsNullOrEmpty(ss) && ss.Equals("true", StringComparison.OrdinalIgnoreCase) && loginResult.Result == AbpLoginResultType.Success)
                        {
                            loginResult.User.SetSignInToken();
                            returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, loginResult.User.SignInToken, loginResult.User.Id, loginResult.User.TenantId);
                        }

                        return Redirect(returnUrl);
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    return await RegisterForExternalLogin(externalLoginInfo);
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        loginResult.Result,
                        externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? externalLoginInfo.ProviderKey,
                        tenancyName
                    );
            }
        }

        private async Task<ActionResult> RegisterForExternalLogin(ExternalLoginInfo externalLoginInfo)
        {
            var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            var nameinfo = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(externalLoginInfo.Principal.Claims.ToList(), _identityOptions);

            var viewModel = new RegisterViewModel
            {
                EmailAddress = email,
                Name = nameinfo.name,
                Surname = nameinfo.surname,
                IsExternalLogin = true,
                ExternalLoginAuthSchema = externalLoginInfo.LoginProvider,
            };

            if (nameinfo.name != null
                && nameinfo.surname != null
                && email != null)
            {
                return await Register(viewModel);
            }

            return await RegisterView(viewModel);
        }

        #endregion

        #region Impersonation

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users_Impersonation)]
        public virtual async Task<JsonResult> ImpersonateUser([FromBody] ImpersonateUserInput input)
        {
            var output = await _accountAppService.ImpersonateUser(input);

            await _signInManager.SignOutAsync();

            return Json(new AjaxResponse
            {
                TargetUrl = _webUrlService.GetSiteRootAddress(output.TenancyName) +
                            "Account/ImpersonateSignIn?tokenId=" + output.ImpersonationToken,
            });
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tenants_Impersonation)]
        public virtual async Task<JsonResult> ImpersonateTenant([FromBody] ImpersonateTenantInput input)
        {
            var output = await _accountAppService.ImpersonateTenant(input);

            await _signInManager.SignOutAsync();

            return Json(new AjaxResponse
            {
                TargetUrl = _webUrlService.GetSiteRootAddress(output.TenancyName) +
                            "Account/ImpersonateSignIn?tokenId=" + output.ImpersonationToken,
            });
        }

        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<ActionResult> ImpersonateSignIn(string tokenId)
        {
            var result = await _impersonationManager.GetImpersonatedUserAndIdentity(tokenId);
            await _signInManager.SignInAsync(result.Identity, false);
            return RedirectToAppHome();
        }

        [AllowAnonymous]
        public virtual JsonResult IsImpersonatedLogin()
        {
            return Json(new AjaxResponse { Result = AbpSession.ImpersonatorUserId.HasValue });
        }

        [AllowAnonymous]
        public virtual async Task<JsonResult> BackToImpersonator()
        {
            var output = await _accountAppService.BackToImpersonator();

            await _signInManager.SignOutAsync();

            return Json(new AjaxResponse
            {
                TargetUrl = _webUrlService.GetSiteRootAddress(output.TenancyName) + "Account/ImpersonateSignIn?tokenId=" + output.ImpersonationToken,
            });
        }

        #endregion

        #region Linked Account

        [UnitOfWork]
        [AbpMvcAuthorize]
        public virtual async Task<JsonResult> SwitchToLinkedAccount([FromBody] SwitchToLinkedAccountInput model)
        {
            var output = await _accountAppService.SwitchToLinkedAccount(model);

            await _signInManager.SignOutAsync();

            return Json(new AjaxResponse
            {
                TargetUrl = _webUrlService.GetSiteRootAddress(output.TenancyName) + "Account/SwitchToLinkedAccountSignIn?tokenId=" + output.SwitchAccountToken,
            });
        }

        [UnitOfWork]
        [AllowAnonymous]
        public virtual async Task<ActionResult> SwitchToLinkedAccountSignIn(string tokenId)
        {
            var result = await _userLinkManager.GetSwitchedUserAndIdentity(tokenId);
            result.User.LastLoginTime = Clock.Now;

            await _signInManager.SignInAsync(result.Identity, false);
            return RedirectToAppHome();
        }

        #endregion

        #region Change Tenant

        [AllowAnonymous]
        public async Task<ActionResult> TenantChangeModal()
        {
            var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
            return View("/Views/Shared/Components/TenantChange/_ChangeModal.cshtml", new ChangeModalViewModel
            {
                TenancyName = loginInfo.Tenant?.TenancyName,
            });
        }

        #endregion

        #region Common

        private async Task<string> GetTenancyNameOrNullAsync()
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (!tenantId.HasValue)
            {
                return null;
            }

            return (await _tenantCache.GetOrNullAsync(tenantId.Value))?.TenancyName;
        }

        private async Task CheckCurrentTenantAsync(int? tenantId)
        {
            var sessionTenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (sessionTenantId != tenantId)
            {
                throw new Exception($"Current tenant is different than given tenant. AbpSession.TenantId: {sessionTenantId}, given tenantId: {tenantId}");
            }
        }

        private async Task SwitchToTenantIfNeeded(string tenancyName)
        {
            var tenantResult = await _accountAppService.IsTenantAvailable(new IsTenantAvailableInput
            {
                TenancyName = tenancyName,
            });

            switch (tenantResult.State)
            {
                case TenantAvailabilityState.Available: break;
                case TenantAvailabilityState.InActive: throw new UserFriendlyException(L("TenantIsNotActive", tenancyName));
                case TenantAvailabilityState.NotFound: throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
            }

            if (tenantResult.TenantId != await AbpSession.GetTenantIdOrNullAsync())
            {
                SetTenantIdCookie(tenantResult.TenantId);
                CurrentUnitOfWork.SetTenantId(tenantResult.TenantId);
                //await _signInManager.SignOutAsync();
            }
        }

        private async Task SwitchToTenantIfNeeded(int? tenantId)
        {
            if (tenantId != await AbpSession.GetTenantIdOrNullAsync())
            {
                if (_webUrlService.SupportsTenancyNameInUrl)
                {
                    throw new InvalidOperationException($"Given tenantid ({tenantId}) does not match to tenant's URL!");
                }

                SetTenantIdCookie(tenantId);
                CurrentUnitOfWork.SetTenantId(tenantId);
                await _signInManager.SignOutAsync();
            }
        }

        #endregion

        #region Helpers

        [AllowAnonymous]
        public ActionResult RedirectToAppHome()
        {
            return RedirectToAction("Index", "Home", new { area = "App" });
        }

        [AllowAnonymous]
        public string GetAppHomeUrl()
        {
            return Url.Action("Index", "Home", new { area = "App" });
        }

        private string NormalizeReturnUrl(string returnUrl, Func<string> defaultValueBuilder = null)
        {
            defaultValueBuilder ??= GetAppHomeUrl;

            if (returnUrl.IsNullOrEmpty())
            {
                return defaultValueBuilder();
            }

            if (Url.IsLocalUrl(returnUrl) || _webUrlService.GetRedirectAllowedExternalWebSites().Any(returnUrl.Contains))
            {
                return returnUrl;
            }

            return defaultValueBuilder();
        }

        #endregion

        #region Etc

        [AbpMvcAuthorize]
        public async Task<ActionResult> TestNotification(string message = "", string severity = "info")
        {
            if (message.IsNullOrEmpty())
            {
                message = "This is a test notification, created at " + Clock.Now;
            }

            await _appNotifier.SendMessageAsync(
                await AbpSession.ToUserIdentifierAsync(),
                message,
                severity.ToPascalCase().ToEnum<NotificationSeverity>()
                );

            return Content("Sent notification: " + message);
        }

        #endregion
    }
}
