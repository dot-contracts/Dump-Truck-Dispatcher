using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Notifications;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Identity;

namespace DispatcherWeb.Authorization.Users
{
    [RemoteService(false)]
    public class UserCreatorService : DispatcherWebAppServiceBase, IUserCreatorService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IUserPolicy _userPolicy;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;

        public UserCreatorService(
            RoleManager roleManager,
            UserManager userManager,
            IUserEmailer userEmailer,
            IUserPolicy userPolicy,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            IPasswordHasher<User> passwordHasher,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userEmailer = userEmailer;
            _userPolicy = userPolicy;
            _passwordValidators = passwordValidators;
            _passwordHasher = passwordHasher;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
            AppUrlService = NullAppUrlService.Instance;
        }

        [RemoteService(false)]
        public async Task<User> CreateUser(CreateOrUpdateUserInput input)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (tenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(tenantId.Value);
            }

            var user = new User
            {
                TenantId = tenantId,
                Name = input.User.Name,
                Surname = input.User.Surname,
                UserName = input.User.UserName,
                EmailAddress = input.User.EmailAddress,
                PhoneNumber = input.User.PhoneNumber,
                OfficeId = input.User.OfficeId,
                IsActive = input.User.IsActive,
                ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin,
                IsTwoFactorEnabled = input.User.IsTwoFactorEnabled,
                IsLockoutEnabled = input.User.IsLockoutEnabled,
                CustomerContactId = input.User.CustomerContactId,
            };

            //Set password
            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(tenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }
                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }
            else
            {
                throw new UserFriendlyException("Password is required");
            }

            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;

            if (input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Customer)
                && user.CustomerContactId == null)
            {
                throw new UserFriendlyException(L("AssigningCustomerRoleManuallyIsNotSupported"));
            }

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.AssignedRoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(tenantId, user.Id, role.Id));
            }

            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

            if (user.OfficeId.HasValue)
            {
                await _officeOrganizationUnitSynchronizer.AddUserToOrganizationUnitForOfficeId(user.Id, user.OfficeId.Value);
            }

            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            await _appNotifier.WelcomeToTheApplicationAsync(user);

            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            //Send activation email
            if (input.SendActivationEmail)
            {
                var emailTemplate = await GetActivationEmailTemplate(input.AssignedRoleNames);

                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    await AppUrlService.CreateEmailActivationUrlFormatAsync(tenantId),
                    input.User.Password,
                    emailTemplate.SubjectTemplate,
                    emailTemplate.BodyTemplate
                );
            }

            return user;
        }

        [RemoteService(false)]
        public async Task<string> ResetUserPassword(User user)
        {
            var randomPassword = await _userManager.CreateRandomPassword();
            user.Password = _passwordHasher.HashPassword(user, randomPassword);
            user.ShouldChangePasswordOnNextLogin = true;
            CheckErrors(await UserManager.UpdateAsync(user));
            return randomPassword;
        }

        [RemoteService(false)]
        public async Task<ActivationEmailTemplateDto> GetActivationEmailTemplate(string[] roleNames)
        {
            var emailTemplate = new ActivationEmailTemplateDto();
            if (roleNames.Contains(StaticRoleNames.Tenants.User))
            {
                emailTemplate.SubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.UserEmailSubjectTemplate);
                emailTemplate.BodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.UserEmailBodyTemplate);
            }
            else if (roleNames.Contains(StaticRoleNames.Tenants.Driver))
            {
                emailTemplate.SubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.DriverEmailSubjectTemplate);
                emailTemplate.BodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.DriverEmailBodyTemplate);
            }
            else if (roleNames.Contains(StaticRoleNames.Tenants.LeaseHaulerDriver))
            {
                emailTemplate.SubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate);
                emailTemplate.BodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate);
            }
            else if (roleNames.Contains(StaticRoleNames.Tenants.Customer))
            {
                emailTemplate.SubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.CustomerPortalEmailSubjectTemplate);
                emailTemplate.BodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.CustomerPortalEmailBodyTemplate);
            }
            return emailTemplate;
        }
    }
}
