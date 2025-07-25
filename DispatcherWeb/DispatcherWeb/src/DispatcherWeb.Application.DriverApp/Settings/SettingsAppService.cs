using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp.Settings.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApp.Settings
{
    [AbpAuthorize]
    public class SettingsAppService : DispatcherWebDriverAppAppServiceBase, ISettingsAppService
    {

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<Driver> _driverRepository;

        public SettingsAppService(
            IAppConfigurationAccessor configurationAccessor,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<Driver> driverRepository
            )
        {
            _appConfiguration = configurationAccessor.Configuration;
            _timeClassificationRepository = timeClassificationRepository;
            _driverRepository = driverRepository;
        }

        public async Task<SettingsDto> Get()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            var result = new SettingsDto
            {
                UseShifts = await SettingManager.UseShifts(),
                ShiftNames = (await SettingManager.GetShiftDictionary()).ToDictionary(x => (int)x.Key, x => x.Value),
                //UseBackgroundSync = (_appConfiguration["App:UseBackgroundSyncForDriverApp"] ?? "true") != "false",
                HttpRequestTimeout = GetDriverApplicationHttpTimeout(),
                HideTicketControls = await SettingManager.HideTicketControlsInDriverApp(),
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
                RequireToEnterTickets = await SettingManager.RequireDriversToEnterTickets(),
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
                RequireSignature = await SettingManager.RequireSignature(),
                RequireTicketPhoto = await SettingManager.RequireTicketPhoto(),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                DriverAppImageResolution = (DriverAppImageResolutionEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.DriverAppImageResolution),
                LocationTimeout = await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LocationTimeout),
                LocationMaxAge = await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LocationMaxAge),
                EnableLocationHighAccuracy = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DriverApp.EnableLocationHighAccuracy),
                AllowEditingTimeOnHourlyJobs = await SettingManager.AllowEditingTimeOnHourlyJobs(),
                //DispatchesLockedToTruck = await SettingManager.DispatchesLockedToTruck(),
                AllowLoadCountOnHourlyJobs = await SettingManager.AllowLoadCountOnHourlyJobs(),
                AutoGenerateTicketNumbers = await SettingManager.AutoGenerateTicketNumbers(),
                DisableTicketNumberOnDriverApp = await SettingManager.DisableTicketNumberOnDriverApp(),
                AllowMultipleDispatchesToBeInProgressAtTheSameTime = await SettingManager.AllowMultipleDispatchesToBeInProgressAtTheSameTime(),
                BasePayOnHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.BasePayOnHourlyJobRate),
                UseDriverSpecificHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate),
                HideDriverAppTimeScreen = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideDriverAppTimeScreen),
                LoggingLevel = (LogLevel)await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LoggingLevel),
                SyncDataOnButtonClicks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DriverApp.SyncDataOnButtonClicks),
                PeriodicSyncCheckIntervalSeconds = await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.PeriodicSyncCheckIntervalSeconds),
                IsUserAdmin = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin)
                    || await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Administrative),
                UserName = user.Name + " " + user.Surname,
                MinimumNativeAppVersion = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.MinimumNativeAppVersion),
                RecommendedNativeAppVersion = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.RecommendedNativeAppVersion),
                GooglePlayUrl = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.GooglePlayUrl),
                AppleStoreUrl = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.AppleStoreUrl),
                SignalRHeartbeatInterval = GetSignalRHeartbeatInterval(),
                Features = new FeaturesDto
                {
                    ReactNativeDriverApp = await FeatureChecker.IsEnabledAsync(AppFeatures.ReactNativeDriverApp),
                    Chat = await FeatureChecker.IsEnabledAsync(AppFeatures.ChatFeature),
                    SeparateMaterialAndFreightItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems),
                    IncludeTravelTime = await FeatureChecker.IsEnabledAsync(AppFeatures.IncludeTravelTime),
                },
                Permissions = new PermissionsDto
                {
                    ReactNativeDriverApp = await IsGrantedAsync(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp),
                    DriverAppSettings = await IsGrantedAsync(AppPermissions.Pages_DriverApplication_Settings),
                },
            };

            result.ProductionPayId = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.IsProductionBased)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var driver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.UserId == user.Id && !x.IsInactive)
                .Select(x => new
                {
                    x.Id,
                    LeaseHaulerId = (int?)x.LeaseHaulerDriver.LeaseHaulerId,
                })
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (driver != null)
            {
                result.DriverId = driver.Id;
                result.IsUserDriver = true;
                result.IsUserLeaseHaulerDriver = driver.LeaseHaulerId != null;
            }

            return result;
        }

        private int GetDriverApplicationHttpTimeout()
        {
            return GetIntFromAppConfigurationOrDefault("App:DriverApplicationHttpRequestTimeout", 60000);
        }

        private int GetSignalRHeartbeatInterval()
        {
            return GetIntFromAppConfigurationOrDefault("SignalR:HeartbeatInterval", 0);
        }

        private int GetIntFromAppConfigurationOrDefault(string appConfigurationKey, int defaultValue)
        {
            var stringValue = _appConfiguration[appConfigurationKey];
            if (!string.IsNullOrEmpty(stringValue) && int.TryParse(stringValue, out var parsedIntValue))
            {
                return parsedIntValue;
            }
            return defaultValue;
        }
    }
}
