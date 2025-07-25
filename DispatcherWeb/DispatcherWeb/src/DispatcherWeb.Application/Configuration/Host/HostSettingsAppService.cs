using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Timing;
using Abp.Zero.Configuration;
using DispatcherWeb.Authentication;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration.Dto;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Editions;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Security;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Dto;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.Timing;
using Microsoft.EntityFrameworkCore;
using EmailSettingsEditDto = DispatcherWeb.Configuration.Host.Dto.EmailSettingsEditDto;

namespace DispatcherWeb.Configuration.Host
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
    public class HostSettingsAppService : SettingsAppServiceBase, IHostSettingsAppService
    {
        public IExternalLoginOptionsCacheManager ExternalLoginOptionsCacheManager { get; set; }

        private readonly EditionManager _editionManager;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDriverSyncRequestStore _driverSyncRequestStore;

        public HostSettingsAppService(
            IEmailSender emailSender,
            EditionManager editionManager,
            ITimeZoneService timeZoneService,
            ISettingDefinitionManager settingDefinitionManager,
            ISyncRequestSender syncRequestSender,
            IDriverSyncRequestStore driverSyncRequestStore,
            IAppConfigurationAccessor configurationAccessor) : base(emailSender, configurationAccessor)
        {
            ExternalLoginOptionsCacheManager = NullExternalLoginOptionsCacheManager.Instance;

            _editionManager = editionManager;
            _timeZoneService = timeZoneService;
            _settingDefinitionManager = settingDefinitionManager;
            _syncRequestSender = syncRequestSender;
            _driverSyncRequestStore = driverSyncRequestStore;
        }

        #region Get Settings

        public async Task<HostSettingsEditDto> GetAllSettings()
        {
            return new HostSettingsEditDto
            {
                General = await GetGeneralSettingsAsync(),
                TenantManagement = await GetTenantManagementSettingsAsync(),
                UserManagement = await GetUserManagementAsync(),
                Email = await GetEmailSettingsAsync(),
                Security = await GetSecuritySettingsAsync(),
                Billing = await GetBillingSettingsAsync(),
                ListCache = await GetListCacheSettingsAsync(),

                OtherSettings = await GetOtherSettingsAsync(),
                ExternalLoginProviderSettings = await GetExternalLoginProviderSettings(),
                Sms = await SettingManager.GetSmsSettingsAsync(),
            };
        }

        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
        {
            var timezone = await SettingManager.GetSettingValueForApplicationAsync(TimingSettingNames.TimeZone);
            var settings = new GeneralSettingsEditDto
            {
                Timezone = timezone,
                TimezoneForComparison = timezone,
                NotificationsEmail = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.NotificationsEmail),
                DriverAppImageResolution = (DriverAppImageResolutionEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.DriverAppImageResolution),
                LinkToResourceCenter = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.LinkToResourceCenter),
                TrainingMeetingRequestLink = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.TrainingMeetingRequestLink),
                SupportRequestLink = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.SupportRequestLink),
                MinimumNativeAppVersion = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.MinimumNativeAppVersion),
                RecommendedNativeAppVersion = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.RecommendedNativeAppVersion),
                GooglePlayUrl = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.GooglePlayUrl),
                AppleStoreUrl = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.AppleStoreUrl),
                InvoicePrintLimit = await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.InvoicePrintLimit),
                TempFileExpirationTime = await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.TempFileExpirationTime),
            };

            var defaultTimeZoneId = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, await AbpSession.GetTenantIdOrNullAsync());
            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        private async Task<TenantManagementSettingsEditDto> GetTenantManagementSettingsAsync()
        {
            var settings = new TenantManagementSettingsEditDto
            {
                AllowSelfRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.AllowSelfRegistration),
                IsNewRegisteredTenantActiveByDefault = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault),
                UseCaptchaOnRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TenantManagement.UseCaptchaOnRegistration),
            };

            var defaultEditionId = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.DefaultEdition);
            if (!string.IsNullOrEmpty(defaultEditionId) && (await _editionManager.FindByIdAsync(Convert.ToInt32(defaultEditionId)) != null))
            {
                settings.DefaultEditionId = Convert.ToInt32(defaultEditionId);
            }

            return settings;
        }

        private async Task<HostUserManagementSettingsEditDto> GetUserManagementAsync()
        {
            return new HostUserManagementSettingsEditDto
            {
                IsEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin),
                SmsVerificationEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SmsVerificationEnabled),
                IsCookieConsentEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsCookieConsentEnabled),
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled),
                UseCaptchaOnLogin = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnLogin),
                AllowUsingGravatarProfilePicture = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowUsingGravatarProfilePicture),
                SessionTimeOutSettings = new SessionTimeOutSettingsEditDto
                {
                    IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.IsEnabled),
                    TimeOutSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.TimeOutSecond),
                    ShowTimeOutNotificationSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond),
                    ShowLockScreenWhenTimedOut = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut),
                },
            };
        }

        private async Task<EmailSettingsEditDto> GetEmailSettingsAsync()
        {
            return new EmailSettingsEditDto
            {
                DefaultFromAddress = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress),
                DefaultFromDisplayName = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName),
                SmtpHost = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host),
                SmtpPort = await SettingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port),
                SmtpUserName = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName),
                SmtpPassword = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password),
                SmtpDomain = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Domain),
                SmtpEnableSsl = await SettingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.EnableSsl),
                SmtpUseDefaultCredentials = await SettingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials),
            };
        }

        private async Task<SecuritySettingsEditDto> GetSecuritySettingsAsync()
        {
            var passwordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit),
                RequireLowercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase),
                RequiredLength = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength),
            };

            var defaultPasswordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = Convert.ToBoolean(_settingDefinitionManager
                    .GetSettingDefinition(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit)
                    .DefaultValue),
                RequireLowercase = Convert.ToBoolean(_settingDefinitionManager
                    .GetSettingDefinition(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase)
                    .DefaultValue),
                RequireNonAlphanumeric = Convert.ToBoolean(_settingDefinitionManager
                    .GetSettingDefinition(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric)
                    .DefaultValue),
                RequireUppercase = Convert.ToBoolean(_settingDefinitionManager
                    .GetSettingDefinition(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase)
                    .DefaultValue),
                RequiredLength = Convert.ToInt32(_settingDefinitionManager
                    .GetSettingDefinition(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength)
                    .DefaultValue),
            };

            return new SecuritySettingsEditDto
            {
                UseDefaultPasswordComplexitySettings = passwordComplexitySetting.Equals(defaultPasswordComplexitySetting),
                PasswordComplexity = passwordComplexitySetting,
                DefaultPasswordComplexity = defaultPasswordComplexitySetting,
                UserLockOut = await GetUserLockOutSettingsAsync(),
                TwoFactorLogin = await GetTwoFactorLoginSettingsAsync(),
                AllowOneConcurrentLoginPerUser = await GetOneConcurrentLoginPerUserSetting(),
            };
        }

        private async Task<HostBillingSettingsEditDto> GetBillingSettingsAsync()
        {
            return new HostBillingSettingsEditDto
            {
                LegalName = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.BillingLegalName),
                Address = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.BillingAddress),
            };
        }

        public async Task<ListCacheSettingsDto> GetListCacheSettingsAsync()
        {
            var result = new ListCacheSettingsDto
            {
                Caches = new List<ListCacheSettingsDto.CacheDto>(),
                GlobalCacheVersion = await SettingManager.GetSettingValueAsync<int>(AppSettings.ListCaches.GlobalCacheVersion),
            };
            foreach (var cacheName in ListCacheNames.All)
            {
                result.Caches.Add(new ListCacheSettingsDto.CacheDto
                {
                    CacheName = cacheName,
                    Backend = await GetListCacheSideSettingsAsync(cacheName, ListCacheSide.Backend),
                    Frontend = await GetListCacheSideSettingsAsync(cacheName, ListCacheSide.Frontend),
                });
            }
            return result;
        }

        private async Task<ListCacheSettingsDto.CacheSideDto> GetListCacheSideSettingsAsync(string cacheName, ListCacheSide cacheSide)
        {
            return new ListCacheSettingsDto.CacheSideDto
            {
                IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.ListCaches.IsEnabled(cacheName, cacheSide)),
                SlidingExpirationTimeMinutes = await SettingManager.GetSettingValueAsync<int>(AppSettings.ListCaches.SlidingExpirationTimeMinutes(cacheName, cacheSide)),
            };
        }

        private async Task<OtherSettingsEditDto> GetOtherSettingsAsync()
        {
            return new OtherSettingsEditDto
            {
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled),
            };
        }

        private async Task<UserLockOutSettingsEditDto> GetUserLockOutSettingsAsync()
        {
            return new UserLockOutSettingsEditDto
            {
                IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled),
                MaxFailedAccessAttemptsBeforeLockout = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout),
                DefaultAccountLockoutSeconds = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds),
            };
        }

        private async Task<TwoFactorLoginSettingsEditDto> GetTwoFactorLoginSettingsAsync()
        {
            var twoFactorLoginSettingsEditDto = new TwoFactorLoginSettingsEditDto
            {
                IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled),
                IsEmailProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled),
                IsSmsProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled),
                IsRememberBrowserEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled),
                IsGoogleAuthenticatorEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled),
            };
            return twoFactorLoginSettingsEditDto;
        }

        private async Task<bool> GetOneConcurrentLoginPerUserSetting()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement
                .AllowOneConcurrentLoginPerUser);
        }

        private async Task<Configuration.Dto.ExternalLoginProviderSettingsEditDto> GetExternalLoginProviderSettings()
        {
            var facebookSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.Facebook);
            var googleSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.Google);
            var twitterSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.Twitter);
            var microsoftSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.Microsoft);

            var openIdConnectSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.OpenIdConnect);
            var openIdConnectMapperClaims = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims);

            var wsFederationSettings = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.Host.WsFederation);
            var wsFederationMapperClaims = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.WsFederationMappedClaims);

            return new Configuration.Dto.ExternalLoginProviderSettingsEditDto
            {
                Facebook = facebookSettings.IsNullOrWhiteSpace()
                    ? new FacebookExternalLoginProviderSettings()
                    : facebookSettings.FromJsonString<FacebookExternalLoginProviderSettings>(),
                Google = googleSettings.IsNullOrWhiteSpace()
                    ? new GoogleExternalLoginProviderSettings()
                    : googleSettings.FromJsonString<GoogleExternalLoginProviderSettings>(),
                Twitter = twitterSettings.IsNullOrWhiteSpace()
                    ? new TwitterExternalLoginProviderSettings()
                    : twitterSettings.FromJsonString<TwitterExternalLoginProviderSettings>(),
                Microsoft = microsoftSettings.IsNullOrWhiteSpace()
                    ? new MicrosoftExternalLoginProviderSettings()
                    : microsoftSettings.FromJsonString<MicrosoftExternalLoginProviderSettings>(),

                OpenIdConnect = openIdConnectSettings.IsNullOrWhiteSpace()
                    ? new OpenIdConnectExternalLoginProviderSettings()
                    : openIdConnectSettings.FromJsonString<OpenIdConnectExternalLoginProviderSettings>(),
                OpenIdConnectClaimsMapping = openIdConnectMapperClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : openIdConnectMapperClaims.FromJsonString<List<JsonClaimMapDto>>(),

                WsFederation = wsFederationSettings.IsNullOrWhiteSpace()
                    ? new WsFederationExternalLoginProviderSettings()
                    : wsFederationSettings.FromJsonString<WsFederationExternalLoginProviderSettings>(),
                WsFederationClaimsMapping = wsFederationMapperClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : wsFederationMapperClaims.FromJsonString<List<JsonClaimMapDto>>(),
            };
        }

        #endregion

        #region Update Settings

        private void AddSettingValue(List<SettingInfo> settingValues, string name, string value)
        {
            settingValues.Add(new SettingInfo(name, value));
        }

        public async Task UpdateAllSettings(HostSettingsEditDto input)
        {
            var settingValues = new List<SettingInfo>();

            await UpdateGeneralSettingsAsync(settingValues, input.General);
            UpdateTenantManagement(settingValues, input.TenantManagement);
            UpdateUserManagementSettings(settingValues, input.UserManagement);
            UpdateSecuritySettings(settingValues, input.Security);
            UpdateEmailSettings(settingValues, input.Email);
            SettingManager.UpdateSmsSettings(settingValues, input.Sms);
            UpdateBillingSettings(settingValues, input.Billing);
            UpdateListCacheSettings(settingValues, input.ListCache);
            UpdateOtherSettings(settingValues, input.OtherSettings);
            UpdateExternalLoginSettings(settingValues, input.ExternalLoginProviderSettings);

            await SettingManager.ChangeSettingsForApplicationAsync(settingValues);

            if (settingValues.Any(s => s.Name.IsIn([
                AppSettings.HostManagement.MinimumNativeAppVersion,
                AppSettings.HostManagement.RecommendedNativeAppVersion,
            ])))
            {
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Settings, new ChangedSettings())
                    .SetSuppressTenantFilter(true));
            }
        }

        private void UpdateOtherSettings(List<SettingInfo> settingValues, OtherSettingsEditDto input)
        {
            if (input == null)
            {
                return;
            }
            AddSettingValue(settingValues, AppSettings.UserManagement.IsQuickThemeSelectEnabled, input.IsQuickThemeSelectEnabled.ToString().ToLowerInvariant());
        }

        private void UpdateBillingSettings(List<SettingInfo> settingValues, HostBillingSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.HostManagement.BillingLegalName, input.LegalName);
            AddSettingValue(settingValues, AppSettings.HostManagement.BillingAddress, input.Address);
        }

        private void UpdateListCacheSettings(List<SettingInfo> settingValues, ListCacheSettingsDto input)
        {
            if (input == null)
            {
                return;
            }

            AddSettingValue(settingValues, AppSettings.ListCaches.GlobalCacheVersion, input.GlobalCacheVersion.ToString(CultureInfo.InvariantCulture));
            foreach (var cache in input.Caches)
            {
                if (!ListCacheNames.All.Contains(cache.CacheName))
                {
                    continue;
                }
                AddSettingValue(settingValues, AppSettings.ListCaches.IsEnabled(cache.CacheName, ListCacheSide.Backend), cache.Backend.IsEnabled.ToString().ToLowerInvariant());
                AddSettingValue(settingValues, AppSettings.ListCaches.SlidingExpirationTimeMinutes(cache.CacheName, ListCacheSide.Backend), cache.Backend.SlidingExpirationTimeMinutes.ToString(CultureInfo.InvariantCulture));
                AddSettingValue(settingValues, AppSettings.ListCaches.IsEnabled(cache.CacheName, ListCacheSide.Frontend), cache.Frontend.IsEnabled.ToString().ToLowerInvariant());
                AddSettingValue(settingValues, AppSettings.ListCaches.SlidingExpirationTimeMinutes(cache.CacheName, ListCacheSide.Frontend), cache.Frontend.SlidingExpirationTimeMinutes.ToString(CultureInfo.InvariantCulture));
            }
        }

        private async Task UpdateGeneralSettingsAsync(List<SettingInfo> settingValues, GeneralSettingsEditDto settings)
        {
            if (Clock.SupportsMultipleTimezone)
            {
                if (settings.Timezone.IsNullOrEmpty())
                {
                    var defaultValue = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, await AbpSession.GetTenantIdOrNullAsync());
                    AddSettingValue(settingValues, TimingSettingNames.TimeZone, defaultValue);
                }
                else
                {
                    AddSettingValue(settingValues, TimingSettingNames.TimeZone, settings.Timezone);
                }
            }
            AddSettingValue(settingValues, AppSettings.HostManagement.NotificationsEmail, settings.NotificationsEmail);
            AddSettingValue(settingValues, AppSettings.HostManagement.LinkToResourceCenter, settings.LinkToResourceCenter);
            AddSettingValue(settingValues, AppSettings.HostManagement.TrainingMeetingRequestLink, settings.TrainingMeetingRequestLink);
            AddSettingValue(settingValues, AppSettings.HostManagement.SupportRequestLink, settings.SupportRequestLink);
            if (settings.MinimumNativeAppVersion != await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.MinimumNativeAppVersion))
            {
                AddSettingValue(settingValues, AppSettings.HostManagement.MinimumNativeAppVersion, settings.MinimumNativeAppVersion);
            }
            if (settings.RecommendedNativeAppVersion != await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.RecommendedNativeAppVersion))
            {
                AddSettingValue(settingValues, AppSettings.HostManagement.RecommendedNativeAppVersion, settings.RecommendedNativeAppVersion);
            }
            AddSettingValue(settingValues, AppSettings.HostManagement.GooglePlayUrl, settings.GooglePlayUrl);
            AddSettingValue(settingValues, AppSettings.HostManagement.AppleStoreUrl, settings.AppleStoreUrl);
            AddSettingValue(settingValues, AppSettings.HostManagement.InvoicePrintLimit, settings.InvoicePrintLimit.ToString());
            AddSettingValue(settingValues, AppSettings.HostManagement.TempFileExpirationTime, settings.TempFileExpirationTime.ToString());
            if (settings.DriverAppImageResolution != (DriverAppImageResolutionEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.DriverAppImageResolution))
            {
                AddSettingValue(settingValues, AppSettings.HostManagement.DriverAppImageResolution, settings.DriverAppImageResolution.ToIntString());
                var tenantIds = await (await TenantManager.GetQueryAsync()).Select(t => t.Id).ToListAsync();
                foreach (var id in tenantIds)
                {
                    await SetDriverAppImageResolutionForTenant(id, settings.DriverAppImageResolution);
                }
            }
        }

        public async Task SetDriverAppImageResolutionForTenant(int tenantId, DriverAppImageResolutionEnum value)
        {
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.HostManagement.DriverAppImageResolution, value.ToIntString());
        }

        public async Task<DriverAppImageResolutionEnum> GetDriverAppImageResolutionForTenant(int tenantId)
        {
            return (DriverAppImageResolutionEnum)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.HostManagement.DriverAppImageResolution, tenantId);
        }

        private void UpdateTenantManagement(List<SettingInfo> settingValues, TenantManagementSettingsEditDto settings)
        {
            AddSettingValue(
                settingValues,
                AppSettings.TenantManagement.AllowSelfRegistration,
                settings.AllowSelfRegistration.ToString().ToLowerInvariant()
            );
            AddSettingValue(
                settingValues,
                AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault,
                settings.IsNewRegisteredTenantActiveByDefault.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.TenantManagement.UseCaptchaOnRegistration,
                settings.UseCaptchaOnRegistration.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.TenantManagement.DefaultEdition,
                settings.DefaultEditionId?.ToString() ?? ""
            );
        }

        private void UpdateUserManagementSettings(List<SettingInfo> settingValues, HostUserManagementSettingsEditDto settings)
        {
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin,
                settings.IsEmailConfirmationRequiredForLogin.ToString().ToLowerInvariant()
            );
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SmsVerificationEnabled,
                settings.SmsVerificationEnabled.ToString().ToLowerInvariant()
            );
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.IsCookieConsentEnabled,
                settings.IsCookieConsentEnabled.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.UseCaptchaOnLogin,
                settings.UseCaptchaOnLogin.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.AllowUsingGravatarProfilePicture,
                settings.AllowUsingGravatarProfilePicture.ToString().ToLowerInvariant()
            );

            UpdateUserManagementSessionTimeOutSettings(settingValues, settings.SessionTimeOutSettings);
        }

        private void UpdateUserManagementSessionTimeOutSettings(List<SettingInfo> settingValues, SessionTimeOutSettingsEditDto settings)
        {
            if (settings == null)
            {
                return;
            }

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.IsEnabled,
                settings.IsEnabled.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.TimeOutSecond,
                settings.TimeOutSecond.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond,
                settings.ShowTimeOutNotificationSecond.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut,
                settings.ShowLockScreenWhenTimedOut.ToString()
            );
        }

        private void UpdateSecuritySettings(List<SettingInfo> settingValues, SecuritySettingsEditDto settings)
        {
            if (settings.UseDefaultPasswordComplexitySettings)
            {
                UpdatePasswordComplexitySettings(settingValues, settings.DefaultPasswordComplexity);
            }
            else
            {
                UpdatePasswordComplexitySettings(settingValues, settings.PasswordComplexity);
            }

            UpdateUserLockOutSettings(settingValues, settings.UserLockOut);
            UpdateTwoFactorLoginSettings(settingValues, settings.TwoFactorLogin);
            UpdateOneConcurrentLoginPerUserSetting(settingValues, settings.AllowOneConcurrentLoginPerUser);
        }

        private void UpdatePasswordComplexitySettings(List<SettingInfo> settingValues, PasswordComplexitySetting settings)
        {
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit,
                settings.RequireDigit.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase,
                settings.RequireLowercase.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric,
                settings.RequireNonAlphanumeric.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase,
                settings.RequireUppercase.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength,
                settings.RequiredLength.ToString()
            );
        }

        private void UpdateUserLockOutSettings(List<SettingInfo> settingValues, UserLockOutSettingsEditDto settings)
        {
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled,
                settings.IsEnabled.ToString().ToLowerInvariant());
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds,
                settings.DefaultAccountLockoutSeconds.ToString());
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout,
                settings.MaxFailedAccessAttemptsBeforeLockout.ToString());
        }

        private void UpdateTwoFactorLoginSettings(List<SettingInfo> settingValues, TwoFactorLoginSettingsEditDto settings)
        {
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled,
                settings.IsEnabled.ToString().ToLowerInvariant());
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled,
                settings.IsEmailProviderEnabled.ToString().ToLowerInvariant());
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled,
                settings.IsSmsProviderEnabled.ToString().ToLowerInvariant());
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled,
                settings.IsGoogleAuthenticatorEnabled.ToString().ToLowerInvariant());
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled,
                settings.IsRememberBrowserEnabled.ToString().ToLowerInvariant());
        }

        private void UpdateEmailSettings(List<SettingInfo> settingValues, EmailSettingsEditDto settings)
        {
            AddSettingValue(settingValues, EmailSettingNames.DefaultFromAddress, settings.DefaultFromAddress);
            AddSettingValue(settingValues, EmailSettingNames.DefaultFromDisplayName, settings.DefaultFromDisplayName);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Host, settings.SmtpHost);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Port, settings.SmtpPort.ToString(CultureInfo.InvariantCulture));
            AddSettingValue(settingValues, EmailSettingNames.Smtp.UserName, settings.SmtpUserName);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Password, settings.SmtpPassword);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Domain, settings.SmtpDomain);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.EnableSsl, settings.SmtpEnableSsl.ToString().ToLowerInvariant());
            AddSettingValue(settingValues, EmailSettingNames.Smtp.UseDefaultCredentials, settings.SmtpUseDefaultCredentials.ToString().ToLowerInvariant());
        }

        private void UpdateOneConcurrentLoginPerUserSetting(List<SettingInfo> settingValues, bool allowOneConcurrentLoginPerUser)
        {
            AddSettingValue(settingValues, AppSettings.UserManagement.AllowOneConcurrentLoginPerUser, allowOneConcurrentLoginPerUser.ToString());
        }

        private void UpdateExternalLoginSettings(List<SettingInfo> settingValues, ExternalLoginProviderSettingsEditDto input)
        {
            if (input == null)
            {
                return;
            }
            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.Facebook,
                input.Facebook == null || !input.Facebook.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.Facebook).DefaultValue
                    : input.Facebook.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.Google,
                input.Google == null || !input.Google.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.Google).DefaultValue
                    : input.Google.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.Twitter,
                input.Twitter == null || !input.Twitter.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.Twitter).DefaultValue
                    : input.Twitter.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.Microsoft,
                input.Microsoft == null || !input.Microsoft.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.Microsoft).DefaultValue
                    : input.Microsoft.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.OpenIdConnect,
                input.OpenIdConnect == null || !input.OpenIdConnect.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.OpenIdConnect).DefaultValue
                    : input.OpenIdConnect.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims,
                input.OpenIdConnectClaimsMapping.IsNullOrEmpty()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims).DefaultValue
                    : input.OpenIdConnectClaimsMapping.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Host.WsFederation,
                input.WsFederation == null || !input.WsFederation.IsValid()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.Host.WsFederation).DefaultValue
                    : input.WsFederation.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.WsFederationMappedClaims,
                input.WsFederationClaimsMapping.IsNullOrEmpty()
                    ? _settingDefinitionManager.GetSettingDefinition(AppSettings.ExternalLoginProvider.WsFederationMappedClaims).DefaultValue
                    : input.WsFederationClaimsMapping.ToJsonString()
            );

            ExternalLoginOptionsCacheManager.ClearCache();
        }

        #endregion

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
        public async Task UpdateSettingValue(string settingName, int? tenantId, long? userId, string value)
        {
            if (userId.HasValue)
            {
                await SettingManager.ChangeSettingForUserAsync(new Abp.UserIdentifier(tenantId, userId.Value), settingName, value);
            }
            else if (tenantId.HasValue)
            {
                await SettingManager.ChangeSettingForTenantAsync(tenantId.Value, settingName, value);
            }
            else
            {
                await SettingManager.ChangeSettingForApplicationAsync(settingName, value);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
        public async Task InvalidateDriverSyncRequestStore(UpdateDriverSyncRequestTimestampInput input)
        {
            await _driverSyncRequestStore.SetAsync(input);
        }
    }
}
