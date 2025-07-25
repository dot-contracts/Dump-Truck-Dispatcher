using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.IdentityFramework;
using Abp.Linq;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.Debugging;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using Microsoft.AspNetCore.Identity;

namespace DispatcherWeb.Authorization.Users
{
    public class UserRegistrationManager : DispatcherWebDomainServiceBase
    {
        public IAbpSession AbpSession { get; set; }
        public IAsyncQueryableExecuter AsyncQueryableExecuter { get; set; }

        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IUserPolicy _userPolicy;

        public UserRegistrationManager(
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IUserEmailer userEmailer,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IUserPolicy userPolicy)
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _userPolicy = userPolicy;

            AbpSession = NullAbpSession.Instance;
            AsyncQueryableExecuter = NullAsyncQueryableExecuter.Instance;
        }

        public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, bool isEmailConfirmed, string emailActivationLink)
        {
            await CheckForTenant();
            await CheckSelfRegistrationIsEnabled();

            var tenant = await GetActiveTenantAsync();
            var isNewRegisteredUserActiveByDefault = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault);

            await _userPolicy.CheckMaxUserCountAsync(tenant.Id);

            var user = new User
            {
                TenantId = tenant.Id,
                Name = name,
                Surname = surname,
                EmailAddress = emailAddress,
                IsActive = isNewRegisteredUserActiveByDefault,
                UserName = userName,
                IsEmailConfirmed = isEmailConfirmed,
                Roles = new List<UserRole>(),
            };

            user.SetNormalizedNames();

            var defaultRoles = await AsyncQueryableExecuter.ToListAsync((await _roleManager.GetQueryAsync()).Where(r => r.IsDefault));
            foreach (var defaultRole in defaultRoles)
            {
                user.Roles.Add(new UserRole(tenant.Id, user.Id, defaultRole.Id));
            }

            await _userManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());
            CheckErrors(await _userManager.CreateAsync(user, plainPassword));
            await CurrentUnitOfWork.SaveChangesAsync();

            if (!user.IsEmailConfirmed)
            {
                user.SetNewEmailConfirmationCode();
                await UserManager.UpdateAsync(user);

                await _userEmailer.SendEmailActivationLinkAsync(user, emailActivationLink);
            }

            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            await _appNotifier.WelcomeToTheApplicationAsync(user);

            return user;
        }

        private async Task CheckForTenant()
        {
            if (!(await AbpSession.GetTenantIdOrNullAsync()).HasValue)
            {
                throw new InvalidOperationException("Can not register host users!");
            }
        }

        private async Task CheckSelfRegistrationIsEnabled()
        {
            if (!await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration))
            {
                throw new UserFriendlyException(L("SelfUserRegistrationIsDisabledMessage_Detail"));
            }
        }

        private async Task<bool> UseCaptchaOnRegistration()
        {
            if (DebugHelper.IsDebug)
            {
                return false;
            }

            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration);
        }

        private async Task<Tenant> GetActiveTenantAsync()
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (!tenantId.HasValue)
            {
                return null;
            }

            return await GetActiveTenantAsync(tenantId.Value);
        }

        private async Task<Tenant> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId);
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

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
