using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.TimeClassifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApplication
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication)]
    public class DriverApplicationAppService : DispatcherWebAppServiceBase, IDriverApplicationAppService
    {
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<DriverApplicationDevice> _deviceRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IDriverApplicationLogRepository _driverApplicationLogRepository;
        private readonly IDriverApplicationAuthProvider _driverApplicationAuthProvider;
        private readonly IPushSubscriptionManager _pushSubscriptionManager;
        private readonly IConfigurationRoot _appConfiguration;

        public DriverApplicationAppService(
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<DriverApplicationDevice> deviceRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IDriverApplicationLogRepository driverApplicationLogRepository,
            IDriverApplicationAuthProvider driverApplicationAuthProvider,
            IPushSubscriptionManager pushSubscriptionManager,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _employeeTimeRepository = employeeTimeRepository;
            _driverRepository = driverRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _deviceRepository = deviceRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _driverApplicationLogRepository = driverApplicationLogRepository;
            _driverApplicationAuthProvider = driverApplicationAuthProvider;
            _pushSubscriptionManager = pushSubscriptionManager;
            _appConfiguration = configurationAccessor.Configuration;
        }

        [AbpAllowAnonymous]
        public async Task<List<ScheduledStartTimeInfo>> GetScheduledStartTimeInfo(GetScheduledStartTimeInfoInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);
            using (Session.Use(authInfo.TenantId, authInfo.UserId))
            {
                var driverId = authInfo.DriverId;
                var today = await GetToday();
                var tomorrow = today.AddDays(1);
                var result = new List<ScheduledStartTimeInfo>
                {
                    await GetScheduledStartTimeInfoFor(driverId, today),
                    await GetScheduledStartTimeInfoFor(driverId, tomorrow),
                };

                foreach (var assignment in result.ToList())
                {
                    if (assignment.NextAssignmentDate.HasValue && !result.Any(r => r.Date == assignment.NextAssignmentDate))
                    {
                        result.Add(await GetScheduledStartTimeInfoFor(driverId, assignment.NextAssignmentDate.Value));
                    }
                }

                return result;
            }
        }

        private async Task<ScheduledStartTimeInfo> GetScheduledStartTimeInfoFor(int driverId, DateTime date)
        {
            var info = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.DriverId == driverId && da.Date == date)
                .OrderBy(da => da.StartTime == null)
                .ThenBy(da => da.StartTime)
                .Select(da => new ScheduledStartTimeInfo
                {
                    Date = date,
                    StartTimeUtc = da.StartTime,
                    TruckCode = da.Truck.TruckCode,
                    HasDriverAssignment = true,
                })
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var timezone = await GetTimezone();

            info ??= await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.Id == driverId)
                    .Select(x => new ScheduledStartTimeInfo
                    {
                        Date = date,
                        TruckCode = x.DefaultTrucks.FirstOrDefault().TruckCode,
                        HasDriverAssignment = false,
                    }).FirstAsync(CancellationTokenProvider.Token);

            if (info.StartTimeUtc.HasValue)
            {
                var startTime = info.Date.Date.Add(info.StartTimeUtc.Value.ConvertTimeZoneTo(timezone).TimeOfDay);
                info.StartTimeUtc = startTime.ConvertTimeZoneFrom(timezone);
            }

            var nextInfo = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.DriverId == driverId && da.Date > date)
                .OrderBy(da => da.Date)
                .Select(da => new
                {
                    Date = da.Date,
                })
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            info.NextAssignmentDate = nextInfo?.Date;

            return info;
        }

        public async Task<GetDriverGuidResult> GetDriverGuid()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.UserId?.ToString());
            var result = new GetDriverGuidResult
            {
                UserId = user.Id,
                IsAdmin = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin)
                    || await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Administrative),
                DriverName = user.Name + " " + user.Surname,
            };

            var driver = await (await _driverRepository.GetQueryAsync())
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (driver == null)
            {
                return result;
            }

            if (driver.Guid == null)
            {
                driver.Guid = Guid.NewGuid();
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            result.IsDriver = true;
            result.DriverGuid = driver.Guid.Value;
            result.DriverId = driver.Id;
            result.DriverName = driver.FirstName + " " + driver.LastName;
            result.DriverLeaseHaulerId = driver.LeaseHaulerDriver?.LeaseHaulerId;

            return result;
        }

        public async Task<DriverAppInfo> PostDriverAppInfo(GetDriverAppInfoInput input) //deprecated since PWA v2.5.0.15
        {
            return await GetDriverAppInfo(input);
        }

        private int GetDriverApplicationHttpTimeout()
        {
            const int defaultTimeout = 60000;
            var httpTimeoutString = _appConfiguration["App:DriverApplicationHttpRequestTimeout"];
            if (!string.IsNullOrEmpty(httpTimeoutString) && int.TryParse(httpTimeoutString, out var httpTimeout))
            {
                return httpTimeout;
            }
            return defaultTimeout;
        }

        [HttpPost]
        public async Task<DriverAppInfo> GetDriverAppInfo(GetDriverAppInfoInput input)
        {
            var driverGuidInfo = await GetDriverGuid();
            var result = new DriverAppInfo
            {
                ElapsedTime = await GetElapsedTime(),
                UseShifts = await SettingManager.UseShifts(),
                UseBackgroundSync = (_appConfiguration["App:UseBackgroundSyncForDriverApp"] ?? "true") != "false",
                HttpRequestTimeout = GetDriverApplicationHttpTimeout(),
                LogLevel = (LogLevel)await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LogLevel),
                LocationTimeout = await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LocationTimeout),
                LocationMaxAge = await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LocationMaxAge),
                EnableLocationHighAccuracy = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DriverApp.EnableLocationHighAccuracy),
                ShiftNames = (await SettingManager.GetShiftDictionary()).ToDictionary(x => (int)x.Key, x => x.Value),
                DriverGuid = driverGuidInfo.DriverGuid,
                DriverName = driverGuidInfo.DriverName,
                DriverLeaseHaulerId = driverGuidInfo.DriverLeaseHaulerId,
                IsDriver = driverGuidInfo.IsDriver,
                IsAdmin = driverGuidInfo.IsAdmin,
                UserId = driverGuidInfo.UserId,
                HideTicketControls = await SettingManager.HideTicketControlsInDriverApp(),
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
                RequireToEnterTickets = await SettingManager.RequireDriversToEnterTickets(),
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
                RequireSignature = await SettingManager.RequireSignature(),
                RequireTicketPhoto = await SettingManager.RequireTicketPhoto(),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                DispatchesLockedToTruck = await SettingManager.DispatchesLockedToTruck(),
                AutoGenerateTicketNumbers = await SettingManager.AutoGenerateTicketNumbers(),
                DisableTicketNumberOnDriverApp = await SettingManager.DisableTicketNumberOnDriverApp(),
                AllowLoadCountOnHourlyJobs = await SettingManager.AllowLoadCountOnHourlyJobs(),
                AllowEditingTimeOnHourlyJobs = await SettingManager.AllowEditingTimeOnHourlyJobs(),
                AllowMultipleDispatchesToBeInProgressAtTheSameTime = await SettingManager.AllowMultipleDispatchesToBeInProgressAtTheSameTime(),
                BasePayOnHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.BasePayOnHourlyJobRate),
                UseDriverSpecificHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate),
                HideDriverAppTimeScreen = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideDriverAppTimeScreen),
                SeparateMaterialAndFreightItemsFeature = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems),
            };

            if (input.RequestNewDeviceId)
            {
                result.DeviceId = await _deviceRepository.InsertAndGetIdAsync(new DriverApplicationDevice
                {
                    Useragent = input.Useragent?.Truncate(EntityStringFieldLengths.DriverApplicationDevice.Useragent),
                    AppVersion = input.AppVersion?.Truncate(EntityStringFieldLengths.DriverApplicationDevice.AppVersion),
                    LastSeen = Clock.Now,
                });
            }
            else if (input.DeviceId.HasValue)
            {
                var device = await (await _deviceRepository.GetQueryAsync())
                    .Where(x => x.Id == input.DeviceId)
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token);

                if (device != null)
                {
                    if (!string.IsNullOrEmpty(input.Useragent))
                    {
                        device.Useragent = input.Useragent?.Truncate(EntityStringFieldLengths.DriverApplicationDevice.Useragent);
                    }
                    device.AppVersion = input.AppVersion?.Truncate(EntityStringFieldLengths.DriverApplicationDevice.AppVersion);
                    device.LastSeen = Clock.Now;
                }
            }

            result.ProductionPayId = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.IsProductionBased)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (driverGuidInfo.DriverId != 0)
            {
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

                result.TimeClassifications = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                    .Where(x => x.DriverId == driverGuidInfo.DriverId)
                    .WhereIf(!allowProductionPay, x => !x.TimeClassification.IsProductionBased)
                    .Select(x => new TimeClassificationDto
                    {
                        Id = x.TimeClassificationId,
                        Name = x.TimeClassification.Name,
                        IsDefault = x.IsDefault,
                    })
                    .OrderByDescending(x => x.IsDefault)
                    .ThenBy(x => x.Name)
                    .ToListAsync(CancellationTokenProvider.Token);
            }

            if (input.PushSubscription != null && driverGuidInfo.DriverId != 0)
            {
                await _pushSubscriptionManager.AddDriverPushSubscription(new AddDriverPushSubscriptionInput
                {
                    PushSubscription = input.PushSubscription,
                    DriverId = driverGuidInfo.DriverId,
                    DeviceId = input.DeviceId ?? result.DeviceId,
                });
            }

            return result;
        }

        private async Task<GetElapsedTimeResult> GetElapsedTime()
        {
            var timeZone = await GetTimezone();
            var todayInLocal = await GetToday();
            var todayInUtc = todayInLocal.ConvertTimeZoneFrom(timeZone);
            var driverTimes = await (await _employeeTimeRepository.GetQueryAsync())
                .Where(et =>
                    et.UserId == Session.GetUserId()
                    && et.StartDateTime >= todayInUtc.AddDays(-1)
                    && (et.EndDateTime == null || et.EndDateTime < todayInUtc.AddDays(1))
                    && et.ManualHourAmount == null
                )
                .Select(x => new
                {
                    x.StartDateTime,
                    x.EndDateTime,
                    x.IsImported,
                })
                .ToListAsync(CancellationTokenProvider.Token);
            var todayDriverTimes = driverTimes.Where(x => x.StartDateTime >= todayInUtc).ToList();
            var committedElapsedSeconds = todayDriverTimes.Where(x => x.EndDateTime != null).Sum(x => (x.EndDateTime.Value - x.StartDateTime).TotalSeconds);
            var uncommittedElapsedSeconds = todayDriverTimes.Where(x => x.EndDateTime == null && !x.IsImported).Sum(x => (Clock.Now - x.StartDateTime).TotalSeconds);
            var lastClockStartTime = driverTimes.FirstOrDefault(x => x.EndDateTime == null && !x.IsImported)?.StartDateTime;
            return new GetElapsedTimeResult
            {
                ClockIsStarted = driverTimes.Any(x => x.EndDateTime == null && !x.IsImported),
                CommittedElapsedSeconds = committedElapsedSeconds,
                CommittedElapsedSecondsForDay = todayInLocal,
                LastClockStartTimeUtc = lastClockStartTime,
            };
        }

        [AbpAllowAnonymous]
        public async Task<List<EmployeeTimeSlimDto>> GetEmployeeTimesForCurrentUser(GetEmployeeTimesForCurrentUserInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);
            using (Session.Use(authInfo.TenantId, authInfo.UserId))
            {
                var timezone = await GetTimezone();
                input.FromDate = input.FromDate.ConvertTimeZoneFrom(timezone);
                input.ToDate = input.ToDate.ConvertTimeZoneFrom(timezone);

                var items = await (await _employeeTimeRepository.GetQueryAsync())
                    .Where(x => x.StartDateTime >= input.FromDate)
                    .Where(x => x.StartDateTime < input.ToDate)
                    .Where(x => x.UserId == authInfo.UserId)
                    .WhereIf(input.UpdatedAfterDateTime.HasValue, x => x.CreationTime > input.UpdatedAfterDateTime.Value || (x.LastModificationTime != null && x.LastModificationTime > input.UpdatedAfterDateTime.Value))
                    .Select(x => new EmployeeTimeSlimDto
                    {
                        Id = x.Id,
                        Guid = x.Guid,
                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        TimeClassificationId = x.TimeClassificationId,
                        //EquipmentId = x.EquipmentId,
                        LastUpdateDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                        IsEditable = x.PayStatementTime == null,
                    }).ToListAsync();

                foreach (var item in items)
                {
                    item.StartDateTime = item.StartDateTime?.ConvertTimeZoneTo(timezone);
                    item.EndDateTime = item.EndDateTime?.ConvertTimeZoneFrom(timezone);
                }

                return items;
            }
        }

        [UnitOfWork(IsDisabled = true)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task RemoveOldDriverApplicationLogs()
        {
            try
            {
                //var targetDate = Clock.Now.AddDays(-15).Date;
                Logger.Info($"RemoveOldDriverApplicationLogs started at {Clock.Now:s}");

                await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
                {
                    IsTransactional = false,
                    Timeout = TimeSpan.FromMinutes(60),
                }, async () =>
                {
                    using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
                    using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                    {
                        //await _driverApplicationLogRepository.DeleteLogsEarlierThanAsync(targetDate);
                        await _driverApplicationLogRepository.DeleteOldLogsAsync();
                    }
                });

                Logger.Info($"RemoveOldDriverApplicationLogs finished at {Clock.Now:s}");
            }
            catch (Exception e)
            {
                Logger.Error("RemoveOldDriverApplicationLogs failed", e);
                throw;
            }
        }
    }
}
