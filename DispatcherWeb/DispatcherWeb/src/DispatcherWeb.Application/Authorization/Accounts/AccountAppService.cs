using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Encryption;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Accounts.Dto;
using DispatcherWeb.Authorization.Delegation;
using DispatcherWeb.Authorization.Impersonation;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Debugging;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.Url;

namespace DispatcherWeb.Authorization.Accounts
{
    [AbpAuthorize]
    public class AccountAppService : DispatcherWebAppServiceBase, IAccountAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        public IRecaptchaValidator RecaptchaValidator { get; set; }

        private readonly ITenantCache _tenantCache;
        private readonly IUserEmailer _userEmailer;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IImpersonationManager _impersonationManager;
        private readonly IUserLinkManager _userLinkManager;
        private readonly IWebUrlService _webUrlService;
        private readonly IEncryptionService _encryptionService;
        private readonly IUserDelegationManager _userDelegationManager;

        public AccountAppService(
            ITenantCache tenantCache,
            IUserEmailer userEmailer,
            UserRegistrationManager userRegistrationManager,
            IImpersonationManager impersonationManager,
            IUserLinkManager userLinkManager,
            IWebUrlService webUrlService,
            IEncryptionService encryptionService,
            IUserDelegationManager userDelegationManager)
        {
            _tenantCache = tenantCache;
            _userEmailer = userEmailer;
            _userRegistrationManager = userRegistrationManager;
            _impersonationManager = impersonationManager;
            _userLinkManager = userLinkManager;
            _webUrlService = webUrlService;
            _encryptionService = encryptionService;
            _userDelegationManager = userDelegationManager;

            AppUrlService = NullAppUrlService.Instance;
            RecaptchaValidator = NullRecaptchaValidator.Instance;
        }

        [AbpAllowAnonymous]
        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
            if (tenant == null)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            if (!tenant.IsActive)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
            }

            return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id, _webUrlService.GetServerRootAddress(input.TenancyName));
        }

        [AbpAllowAnonymous]
        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            if (await UseCaptchaOnRegistration())
            {
                await RecaptchaValidator.ValidateAsync(input.CaptchaResponse);
            }

            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                false,
                await AppUrlService.CreateEmailActivationUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync())
            );

            var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin),
            };
        }

        [AbpAllowAnonymous]
        public async Task SendPasswordResetCode(SendPasswordResetCodeInput input)
        {
            var user = await UserManager.FindByEmailAsync(input.EmailAddress);
            if (user == null)
            {
                return;
            }

            user.SetNewPasswordResetCode();
            await UserManager.UpdateAsync(user);

            await _userEmailer.SendPasswordResetLinkAsync(
                user,
                await AppUrlService.CreatePasswordResetUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync())
            );
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        public async Task<ResetPasswordOutput> ResetPassword(ResetPasswordInput input)
        {
            var user = await UserManager.GetUserByIdAsync(input.UserId);
            if (user == null || user.PasswordResetCode.IsNullOrEmpty() || user.PasswordResetCode != input.ResetCode)
            {
                throw new UserFriendlyException(L("InvalidPasswordResetCode"), L("InvalidPasswordResetCode_Detail"));
            }

            await UserManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());
            CheckErrors(await UserManager.ChangePasswordAsync(user, input.Password));
            user.PasswordResetCode = null;
            user.IsEmailConfirmed = true;
            user.ShouldChangePasswordOnNextLogin = false;

            await UserManager.UpdateAsync(user);

            return new ResetPasswordOutput
            {
                CanLogin = user.IsActive,
                UserName = user.UserName,
            };
        }

        [AbpAllowAnonymous]
        public async Task SendEmailActivationLink(SendEmailActivationLinkInput input)
        {
            var user = await UserManager.FindByEmailAsync(input.EmailAddress);
            if (user == null)
            {
                return;
            }

            user.SetNewEmailConfirmationCode();
            await UserManager.UpdateAsync(user);

            await _userEmailer.SendEmailActivationLinkAsync(
                user,
                await AppUrlService.CreateEmailActivationUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync())
            );
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        public async Task ActivateEmail(ActivateEmailInput input)
        {
            var user = await UserManager.FindByIdAsync(input.UserId.ToString());
            if (user != null && user.IsEmailConfirmed)
            {
                return;
            }

            if (user == null || user.EmailConfirmationCode.IsNullOrEmpty() || user.EmailConfirmationCode != input.ConfirmationCode)
            {
                throw new UserFriendlyException(L("InvalidEmailConfirmationCode"), L("InvalidEmailConfirmationCode_Detail"));
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationCode = null;

            await UserManager.UpdateAsync(user);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Impersonation)]
        public virtual async Task<ImpersonateOutput> ImpersonateUser(ImpersonateUserInput input)
        {
            return new ImpersonateOutput
            {
                ImpersonationToken = await _impersonationManager.GetImpersonationToken(input.UserId, await AbpSession.GetTenantIdOrNullAsync()),
                TenancyName = await GetTenancyNameOrNullAsync(input.TenantId),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Impersonation)]
        public virtual async Task<ImpersonateOutput> ImpersonateTenant(ImpersonateTenantInput input)
        {
            return new ImpersonateOutput
            {
                ImpersonationToken = await _impersonationManager.GetImpersonationToken(input.UserId, input.TenantId),
                TenancyName = await GetTenancyNameOrNullAsync(input.TenantId),
            };
        }

        public virtual async Task<ImpersonateOutput> DelegatedImpersonate(DelegatedImpersonateInput input)
        {
            var userDelegation = await _userDelegationManager.GetAsync(input.UserDelegationId);
            if (userDelegation.TargetUserId != await AbpSession.GetUserIdAsync())
            {
                throw new UserFriendlyException("User delegation error.");
            }

            return new ImpersonateOutput
            {
                ImpersonationToken = await _impersonationManager.GetImpersonationToken(userDelegation.SourceUserId, userDelegation.TenantId),
                TenancyName = await GetTenancyNameOrNullAsync(userDelegation.TenantId),
            };
        }

        public virtual async Task<ImpersonateOutput> BackToImpersonator()
        {
            return new ImpersonateOutput
            {
                ImpersonationToken = await _impersonationManager.GetBackToImpersonatorToken(),
                TenancyName = await GetTenancyNameOrNullAsync(AbpSession.ImpersonatorTenantId),
            };
        }

        public virtual async Task<SwitchToLinkedAccountOutput> SwitchToLinkedAccount(SwitchToLinkedAccountInput input)
        {
            if (!await _userLinkManager.AreUsersLinked(await AbpSession.ToUserIdentifierAsync(), input.ToUserIdentifier()))
            {
                throw new Exception(L("This account is not linked to your account"));
            }

            return new SwitchToLinkedAccountOutput
            {
                SwitchAccountToken = await _userLinkManager.GetAccountSwitchToken(input.TargetUserId, input.TargetTenantId),
                TenancyName = await GetTenancyNameOrNullAsync(input.TargetTenantId),
            };
        }

        private async Task<bool> UseCaptchaOnRegistration()
        {
            if (DebugHelper.IsDebug)
            {
                return false;
            }

            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration);
        }

        private async Task<TenantCacheItem> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantCache.GetOrNullAsync(tenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }

        private async Task<string> GetTenancyNameOrNullAsync(int? tenantId)
        {
            return tenantId.HasValue ? (await GetActiveTenantAsync(tenantId.Value)).TenancyName : null;
        }

        private async Task<User> GetUserByChecking(string inputEmailAddress)
        {
            var user = await UserManager.FindByEmailAsync(inputEmailAddress);
            if (user == null)
            {
                throw new UserFriendlyException(L("InvalidEmailAddress"));
            }

            return user;
        }
    }
}
