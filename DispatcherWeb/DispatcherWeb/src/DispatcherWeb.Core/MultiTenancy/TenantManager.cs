using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Abp;
using Abp.Application.Features;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Notifications;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Editions;
using DispatcherWeb.MultiTenancy.Demo;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy
{
    /// <summary>
    /// Tenant manager.
    /// </summary>
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public IAbpSession AbpSession { get; set; }

        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IUserEmailer _userEmailer;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<SubscribableEdition> _subscribableEditionRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<Office> _officeRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ISettingManager _settingManager;

        public TenantManager(
            ICacheManager cacheManager,
            IRepository<Tenant> tenantRepository,
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository,
            EditionManager editionManager,
            IUnitOfWorkManager unitOfWorkManager,
            RoleManager roleManager,
            IUserEmailer userEmailer,
            UserManager userManager,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IAbpZeroFeatureValueStore featureValueStore,
            IAbpZeroDbMigrator abpZeroDbMigrator,
            IPasswordHasher<User> passwordHasher,
            IRepository<SubscribableEdition> subscribableEditionRepository,
            IBackgroundJobManager backgroundJobManager,
            IRepository<Office> officeRepository,
            IEncryptionService encryptionService,
            ISettingManager settingManager
        ) : base(
            tenantRepository,
            tenantFeatureRepository,
            editionManager,
            featureValueStore
        )
        {
            AbpSession = NullAbpSession.Instance;
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _userManager = userManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            _passwordHasher = passwordHasher;
            _subscribableEditionRepository = subscribableEditionRepository;
            _backgroundJobManager = backgroundJobManager;
            _officeRepository = officeRepository;
            _encryptionService = encryptionService;
            _settingManager = settingManager;
        }

        private async Task<string> GetUniqueTenancyName(string companyName)
        {
            var tenancyNameBase = string.Join("", companyName.Trim().Replace(' ', '-').Where(x => char.IsLetterOrDigit(x) || x == '-'));
            int? tenancyNameSuffix = null;

            do
            {
                var tenancyName = tenancyNameBase + tenancyNameSuffix;
                if (!await (await GetQueryAsync()).AnyAsync(x => x.TenancyName == tenancyName))
                {
                    return tenancyName;
                }
                tenancyNameSuffix = tenancyNameSuffix == null ? 2 : tenancyNameSuffix.Value + 1;
            }
            while (true);
        }

        public async Task<int> CreateWithAdminUserAsync(
            string companyName,
            string adminFirstName,
            string adminLastName,
            string adminPassword,
            string adminEmailAddress,
            string connectionString,
            bool isActive,
            int? editionId,
            bool shouldChangePasswordOnNextLogin,
            bool sendActivationEmail,
            DateTime? subscriptionEndDate,
            bool isInTrialPeriod,
            Func<string, string> getActivationUrlFromTenancyName)
        {
            int newTenantId = 0;
            long newAdminId = 0;

            await CheckEditionAsync(editionId, isInTrialPeriod);

            if (isInTrialPeriod && !subscriptionEndDate.HasValue)
            {
                throw new UserFriendlyException(LocalizationManager.GetString(
                    DispatcherWebConsts.LocalizationSourceName, "TrialWithoutEndDateErrorMessage"));
            }

            await _unitOfWorkManager.WithUnitOfWorkAsync(TransactionScopeOption.RequiresNew, async () =>
            {
                var tenancyName = await GetUniqueTenancyName(companyName);

                //Create tenant
                var tenant = new Tenant(tenancyName, companyName)
                {
                    IsActive = isActive,
                    EditionId = editionId,
                    SubscriptionEndDateUtc = subscriptionEndDate?.ToUniversalTime(),
                    IsInTrialPeriod = isInTrialPeriod,
                    ConnectionString = connectionString.IsNullOrWhiteSpace()
                        ? null
                        : _encryptionService.EncryptIfNotEmpty(connectionString),
                };

                await CreateAsync(tenant);
                await _unitOfWorkManager.Current.SaveChangesAsync(); //To get new tenant's id.

                //Create tenant database
                await _abpZeroDbMigrator.CreateOrMigrateForTenantAsync(tenant);

                //We are working entities of new tenant, so changing tenant filter
                using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
                {
                    await _settingManager.ChangeSettingForTenantAsync(tenant.Id, AppSettings.General.CompanyName, companyName);

                    //Create static roles for new tenant
                    CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
                    await _unitOfWorkManager.Current.SaveChangesAsync(); //To get static role ids

                    //User role should be default
                    var userRole = (await _roleManager.GetQueryAsync()).Single(r => r.Name == StaticRoleNames.Tenants.User);
                    userRole.IsDefault = true;
                    CheckErrors(await _roleManager.UpdateAsync(userRole));

                    //Create admin user for the tenant
                    var adminUser = User.CreateTenantAdminUser(tenant.Id, adminFirstName, adminLastName, adminEmailAddress);
                    adminUser.ShouldChangePasswordOnNextLogin = shouldChangePasswordOnNextLogin;
                    adminUser.IsActive = true;

                    if (adminPassword.IsNullOrEmpty())
                    {
                        adminPassword = await _userManager.CreateRandomPassword();
                    }
                    else
                    {
                        await _userManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());
                        foreach (var validator in _userManager.PasswordValidators)
                        {
                            CheckErrors(await validator.ValidateAsync(_userManager, adminUser, adminPassword));
                        }
                    }

                    adminUser.Password = _passwordHasher.HashPassword(adminUser, adminPassword);

                    CheckErrors(await _userManager.CreateAsync(adminUser));
                    await _unitOfWorkManager.Current.SaveChangesAsync(); //To get admin user's id

                    await CreateDefaultOffice(adminUser);
                    //Assign admin user to admin role
                    var adminRole = (await _roleManager.GetQueryAsync()).Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));

                    //Notifications
                    await _appNotifier.WelcomeToTheApplicationAsync(adminUser);

                    //Send activation email
                    if (sendActivationEmail)
                    {
                        adminUser.SetNewEmailConfirmationCode();
                        await _userManager.UpdateAsync(adminUser);
                        var emailActivationLink = getActivationUrlFromTenancyName(tenancyName);
                        await _userEmailer.SendEmailActivationLinkAsync(adminUser, emailActivationLink, adminPassword);
                    }

                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    await _backgroundJobManager.EnqueueAsync<TenantDemoDataBuilderJob, int>(tenant.Id);

                    newTenantId = tenant.Id;
                    newAdminId = adminUser.Id;
                }
            });

            //Used a second UOW since UOW above sets some permissions and _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync needs these permissions to be saved.
            await _unitOfWorkManager.WithUnitOfWorkAsync(TransactionScopeOption.RequiresNew, async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(newTenantId))
                {
                    await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(
                        new UserIdentifier(newTenantId, newAdminId));
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                }
            });

            return newTenantId;
        }

        protected override Task ValidateTenancyNameAsync(string tenancyName)
        {
            ValidateTenancyName(tenancyName);
            return Task.CompletedTask;
        }

        protected override void ValidateTenancyName(string tenancyName)
        {
            if (!Regex.IsMatch(tenancyName, Tenant.TenancyNameRegex))
            {
                throw new UserFriendlyException(L("InvalidTenancyName"));
            }
        }

        private async Task CreateDefaultOffice(User adminUser)
        {
            const string name = "Main";
            const string truckColor = "#e6e3e4";

            var office = new Office
            {
                Name = name,
                TruckColor = truckColor,
                DefaultStartTime = new DateTime(2000, 1, 1, 12, 0, 0), //12pm UTC / 7am EST
            };
            office.Users.Add(adminUser);
            await _officeRepository.InsertAsync(office);
        }

        public async Task CheckEditionAsync(int? editionId, bool isInTrialPeriod)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                if (!editionId.HasValue || !isInTrialPeriod)
                {
                    return;
                }

                var edition = await _subscribableEditionRepository.GetAsync(editionId.Value);
                if (!edition.IsFree)
                {
                    return;
                }

                var error = LocalizationManager.GetSource(DispatcherWebConsts.LocalizationSourceName)
                    .GetString("FreeEditionsCannotHaveTrialVersions");
                throw new UserFriendlyException(error);
            });
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public decimal GetUpgradePrice(SubscribableEdition currentEdition, SubscribableEdition targetEdition,
            int totalRemainingHourCount, PaymentPeriodType paymentPeriodType)
        {
            int numberOfHoursPerDay = 24;

            var totalRemainingDayCount = totalRemainingHourCount / numberOfHoursPerDay;
            var unusedPeriodCount = totalRemainingDayCount / (int)paymentPeriodType;
            var unusedHoursCount = totalRemainingHourCount % ((int)paymentPeriodType * numberOfHoursPerDay);

            decimal currentEditionPriceForUnusedPeriod = 0;
            decimal targetEditionPriceForUnusedPeriod = 0;

            var currentEditionPrice = currentEdition.GetPaymentAmount(paymentPeriodType);
            var targetEditionPrice = targetEdition.GetPaymentAmount(paymentPeriodType);

            if (currentEditionPrice > 0)
            {
                currentEditionPriceForUnusedPeriod = currentEditionPrice * unusedPeriodCount;
                currentEditionPriceForUnusedPeriod += (currentEditionPrice / (int)paymentPeriodType) /
                    numberOfHoursPerDay * unusedHoursCount;
            }

            if (targetEditionPrice > 0)
            {
                targetEditionPriceForUnusedPeriod = targetEditionPrice * unusedPeriodCount;
                targetEditionPriceForUnusedPeriod += (targetEditionPrice / (int)paymentPeriodType) /
                    numberOfHoursPerDay * unusedHoursCount;
            }

            return targetEditionPriceForUnusedPeriod - currentEditionPriceForUnusedPeriod;
        }

        public async Task<Tenant> UpdateTenantAsync(int tenantId, bool isActive, bool? isInTrialPeriod,
            PaymentPeriodType? paymentPeriodType, int editionId, EditionPaymentType editionPaymentType)
        {
            var tenant = await FindByIdAsync(tenantId);

            tenant.IsActive = isActive;

            if (isInTrialPeriod.HasValue)
            {
                tenant.IsInTrialPeriod = isInTrialPeriod.Value;
            }

            tenant.EditionId = editionId;

            if (paymentPeriodType.HasValue)
            {
                tenant.UpdateSubscriptionDateForPayment(paymentPeriodType.Value, editionPaymentType);
            }

            return tenant;
        }

        public async Task<EndSubscriptionResult> EndSubscriptionAsync(Tenant tenant, SubscribableEdition edition,
            DateTime nowUtc)
        {
            if (tenant.EditionId == null || tenant.HasUnlimitedTimeSubscription())
            {
                throw new Exception(
                    $"Can not end tenant {tenant.TenancyName} subscription for {edition.DisplayName} tenant has unlimited time subscription!");
            }

            Debug.Assert(tenant.SubscriptionEndDateUtc != null, "tenant.SubscriptionEndDateUtc != null");

            var subscriptionEndDateUtc = tenant.SubscriptionEndDateUtc.Value;
            if (!tenant.IsInTrialPeriod)
            {
                subscriptionEndDateUtc =
                    tenant.SubscriptionEndDateUtc.Value.AddDays(edition.WaitingDayAfterExpire ?? 0);
            }

            if (subscriptionEndDateUtc >= nowUtc)
            {
                throw new Exception(
                    $"Can not end tenant {tenant.TenancyName} subscription for {edition.DisplayName} since subscription has not expired yet!");
            }

            if (!tenant.IsInTrialPeriod && edition.ExpiringEditionId.HasValue)
            {
                tenant.EditionId = edition.ExpiringEditionId.Value;
                tenant.SubscriptionEndDateUtc = null;

                await UpdateAsync(tenant);

                return EndSubscriptionResult.AssignedToAnotherEdition;
            }

            tenant.IsActive = false;
            tenant.IsInTrialPeriod = false;

            await UpdateAsync(tenant);

            return EndSubscriptionResult.TenantSetInActive;
        }

        public override Task UpdateAsync(Tenant tenant)
        {
            if (tenant.IsInTrialPeriod && !tenant.SubscriptionEndDateUtc.HasValue)
            {
                throw new UserFriendlyException(LocalizationManager.GetString(
                    DispatcherWebConsts.LocalizationSourceName, "TrialWithoutEndDateErrorMessage"));
            }

            return base.UpdateAsync(tenant);
        }

        public override async Task SetFeatureValuesAsync(int tenantId, params NameValue[] values)
        {
            await base.SetFeatureValuesAsync(tenantId, values);
            await ClearPermissionCache();
        }

        public override async Task ResetAllFeaturesAsync(int tenantId)
        {
            await base.ResetAllFeaturesAsync(tenantId);
            await ClearPermissionCache();
        }

        private async Task ClearPermissionCache()
        {
            //We can clear specific "roleId@tenantId" values instead, but we need a way to either get all cache keys matching "*@tenantId" or iterate through all static roles
            await _cacheManager.GetRolePermissionCache().ClearAsync();
        }
    }
}
