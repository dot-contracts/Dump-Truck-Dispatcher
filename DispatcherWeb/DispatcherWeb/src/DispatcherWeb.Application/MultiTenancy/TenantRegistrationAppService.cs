using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.Editions;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.Url;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy
{
    [AbpAuthorize]
    public class TenantRegistrationAppService : DispatcherWebAppServiceBase, ITenantRegistrationAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IRecaptchaValidator _recaptchaValidator;
        private readonly EditionManager _editionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly ILocalizationContext _localizationContext;
        private readonly TenantManager _tenantManager;
        private readonly ITenantCache _tenantCache;
        private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;

        public TenantRegistrationAppService(
            IMultiTenancyConfig multiTenancyConfig,
            IRecaptchaValidator recaptchaValidator,
            EditionManager editionManager,
            IAppNotifier appNotifier,
            ILocalizationContext localizationContext,
            TenantManager tenantManager,
            ITenantCache tenantCache,
            ISubscriptionPaymentRepository subscriptionPaymentRepository,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer
        )
        {
            _multiTenancyConfig = multiTenancyConfig;
            _recaptchaValidator = recaptchaValidator;
            _editionManager = editionManager;
            _appNotifier = appNotifier;
            _localizationContext = localizationContext;
            _tenantManager = tenantManager;
            _tenantCache = tenantCache;
            _subscriptionPaymentRepository = subscriptionPaymentRepository;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
            AppUrlService = NullAppUrlService.Instance;
        }

        [AbpAllowAnonymous]
        public async Task<RegisterTenantOutput> RegisterTenant(RegisterTenantInput input)
        {
            if (input.EditionId.HasValue)
            {
                await CheckEditionSubscriptionAsync(input.EditionId.Value, input.SubscriptionStartType);
            }
            else
            {
                await CheckRegistrationWithoutEdition();
            }

            using (CurrentUnitOfWork.SetTenantId(null))
            {
                await CheckTenantRegistrationIsEnabled();

                if (await UseCaptchaOnRegistration())
                {
                    await _recaptchaValidator.ValidateAsync(input.CaptchaResponse);
                }

                //Getting host-specific settings
                var isActive = await IsNewRegisteredTenantActiveByDefault(input.SubscriptionStartType);
                var isEmailConfirmationRequired = await SettingManager.GetSettingValueForApplicationAsync<bool>(
                    AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin
                );

                DateTime? subscriptionEndDate = null;
                var isInTrialPeriod = false;

                if (input.EditionId.HasValue)
                {
                    isInTrialPeriod = input.SubscriptionStartType == SubscriptionStartType.Trial;

                    if (isInTrialPeriod)
                    {
                        var edition = (SubscribableEdition)await _editionManager.GetByIdAsync(input.EditionId.Value);
                        subscriptionEndDate = Clock.Now.AddDays(edition.TrialDayCount ?? 0);
                    }
                }

                var tenantId = await _tenantManager.CreateWithAdminUserAsync(
                    input.CompanyName,
                    input.AdminFirstName,
                    input.AdminLastName,
                    input.AdminPassword,
                    input.AdminEmailAddress,
                    null,
                    isActive,
                    input.EditionId,
                    shouldChangePasswordOnNextLogin: false,
                    sendActivationEmail: true,
                    subscriptionEndDate,
                    isInTrialPeriod,
                    AppUrlService.CreateEmailActivationUrlFormat
                );
                await _officeOrganizationUnitSynchronizer.MigrateOfficesForTenant(tenantId);

                var tenant = await TenantManager.GetByIdAsync(tenantId);
                await _appNotifier.NewTenantRegisteredAsync(tenant);

                return new RegisterTenantOutput
                {
                    TenantId = tenant.Id,
                    TenancyName = tenant.TenancyName,
                    Name = tenant.Name,
                    UserName = AbpUserBase.AdminUserName,
                    EmailAddress = input.AdminEmailAddress,
                    IsActive = tenant.IsActive,
                    IsEmailConfirmationRequired = isEmailConfirmationRequired,
                    IsTenantActive = tenant.IsActive,
                };
            }
        }

        private async Task<bool> IsNewRegisteredTenantActiveByDefault(SubscriptionStartType subscriptionStartType)
        {
            if (subscriptionStartType == Payments.SubscriptionStartType.Paid)
            {
                return false;
            }

            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault);
        }

        private async Task CheckRegistrationWithoutEdition()
        {
            if (await (await _editionManager.GetQueryAsync()).AnyAsync())
            {
                throw new Exception("Tenant registration is not allowed without edition because there are editions defined !");
            }
        }

        [AbpAllowAnonymous]
        public async Task<EditionsSelectOutput> GetEditionsForSelect()
        {
            var features = FeatureManager.GetAll()
                .Where(feature => (feature[FeatureMetadata.CustomFeatureKey] as FeatureMetadata)?.IsVisibleOnPricingTable ?? false)
                .ToList();

            var flatFeatures = features
                .Select(x => new FlatFeatureSelectDto
                {
                    ParentName = x.Parent?.Name,
                    Name = x.Name,
                    DisplayName = L(x.DisplayName),
                    Description = L(x.Description),
                    DefaultValue = x.DefaultValue,
                    InputType = x.InputType,
                })
                .OrderBy(x => x.DisplayName)
                .ToList();

            var editions = (await (await _editionManager.GetQueryAsync()).AsNoTracking().ToListAsync())
                .Cast<SubscribableEdition>()
                .Select(x => new EditionSelectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    IsFree = x.IsFree,
                    MonthlyPrice = x.MonthlyPrice,
                    TrialDayCount = x.TrialDayCount,
                    DailyPrice = x.DailyPrice,
                    ExpiringEditionId = x.ExpiringEditionId,
                    WeeklyPrice = x.WeeklyPrice,
                    AnnualPrice = x.AnnualPrice,
                    WaitingDayAfterExpire = x.WaitingDayAfterExpire,
                })
                .OrderBy(e => e.MonthlyPrice)
                .ToList();

            var featureDictionary = features.ToDictionary(feature => feature.Name, f => f);

            var editionWithFeatures = new List<EditionWithFeaturesDto>();
            foreach (var edition in editions)
            {
                editionWithFeatures.Add(await CreateEditionWithFeaturesDto(edition, featureDictionary));
            }

            if (AbpSession.UserId.HasValue)
            {
                var currentEditionId = (await _tenantCache.GetAsync(await AbpSession.GetTenantIdAsync())).EditionId;
                if (currentEditionId.HasValue)
                {
                    editionWithFeatures = editionWithFeatures.Where(e => e.Edition.Id != currentEditionId).ToList();

                    var currentEdition = (SubscribableEdition)(await _editionManager.GetByIdAsync(currentEditionId.Value));
                    if (!currentEdition.IsFree)
                    {
                        var lastPayment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(
                            await AbpSession.GetTenantIdAsync(),
                            null,
                            null))
                            .Select(x => new
                            {
                                x.PaymentPeriodType,
                            }).FirstOrDefaultAsync();

                        if (lastPayment != null)
                        {
                            editionWithFeatures = editionWithFeatures
                                .Where(e =>
                                    e.Edition.GetPaymentAmount(lastPayment.PaymentPeriodType) >
                                    currentEdition.GetPaymentAmount(lastPayment.PaymentPeriodType)
                                )
                                .ToList();
                        }
                    }
                }
            }

            return new EditionsSelectOutput
            {
                AllFeatures = flatFeatures,
                EditionsWithFeatures = editionWithFeatures,
            };
        }

        [AbpAllowAnonymous]
        public async Task<EditionSelectDto> GetEdition(int editionId)
        {
            var edition = await (await _editionManager.GetQueryAsync())
                .Where(x => x.Id == editionId)
                .Select(x => new EditionSelectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                })
                .FirstAsync();

            return edition;
        }

        private async Task<EditionWithFeaturesDto> CreateEditionWithFeaturesDto(EditionSelectDto edition,
            Dictionary<string, Feature> featureDictionary)
        {
            return new EditionWithFeaturesDto
            {
                Edition = edition,
                FeatureValues = (await _editionManager.GetFeatureValuesAsync(edition.Id))
                    .Where(featureValue => featureDictionary.ContainsKey(featureValue.Name))
                    .Select(fv => new NameValueDto(
                        fv.Name,
                        featureDictionary[fv.Name].GetValueText(fv.Value, _localizationContext))
                    )
                    .ToList(),
            };
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

        private async Task<bool> IsSelfRegistrationEnabled()
        {
            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.TenantManagement.AllowSelfRegistration);
        }

        private async Task<bool> UseCaptchaOnRegistration()
        {
            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.TenantManagement.UseCaptchaOnRegistration);
        }

        private async Task CheckEditionSubscriptionAsync(int editionId, SubscriptionStartType subscriptionStartType)
        {
            var edition = await _editionManager.GetByIdAsync(editionId) as SubscribableEdition;

            CheckSubscriptionStart(edition, subscriptionStartType);
        }

        private static void CheckSubscriptionStart(SubscribableEdition edition, SubscriptionStartType subscriptionStartType)
        {
            switch (subscriptionStartType)
            {
                case SubscriptionStartType.Free:
                    if (!edition.IsFree)
                    {
                        throw new Exception("This is not a free edition !");
                    }
                    break;
                case SubscriptionStartType.Trial:
                    if (!edition.HasTrial())
                    {
                        throw new Exception("Trial is not available for this edition !");
                    }
                    break;
                case SubscriptionStartType.Paid:
                    if (edition.IsFree)
                    {
                        throw new Exception("This is a free edition and cannot be subscribed as paid !");
                    }
                    break;
            }
        }
    }
}
