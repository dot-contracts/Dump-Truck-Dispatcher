using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureTables;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using DispatcherWeb.TruckPositions;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleCategories;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Trucks
{
    [AbpAuthorize(AppPermissions.Pages_Trucks)]
    public class TruckTelematicsAppService : DispatcherWebAppServiceBase, ITruckTelematicsAppService
    {
        private readonly IGeotabTelematics _geotabTelematics;
        private readonly ISamsaraTelematics _samsaraTelematics;
        private readonly IDtdTrackerTelematics _dtdTrackerTelematics;
        private readonly IIntelliShiftTelematics _intelliShiftTelematics;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<WialonDeviceType, long> _wialonDeviceTypeRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IAzureTableManager _azureTableManager;
        private readonly ITruckCommonDomainService _truckCommonDomainService;

        public TruckTelematicsAppService(
            IGeotabTelematics geotabTelematics,
            ISamsaraTelematics samsaraTelematics,
            IDtdTrackerTelematics dtdTrackerTelematics,
            IIntelliShiftTelematics intelliShiftTelematics,
            INotificationPublisher notificationPublisher,
            IBackgroundJobManager backgroundJobManager,
            IRepository<Truck> truckRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<WialonDeviceType, long> wialonDeviceTypeRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IAzureTableManager azureTableManager,
            ITruckCommonDomainService truckCommonDomainService
        )
        {
            _geotabTelematics = geotabTelematics;
            _samsaraTelematics = samsaraTelematics;
            _dtdTrackerTelematics = dtdTrackerTelematics;
            _intelliShiftTelematics = intelliShiftTelematics;
            _notificationPublisher = notificationPublisher;
            _backgroundJobManager = backgroundJobManager;
            _truckRepository = truckRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
            _tenantRepository = tenantRepository;
            _wialonDeviceTypeRepository = wialonDeviceTypeRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _azureTableManager = azureTableManager;
            _truckCommonDomainService = truckCommonDomainService;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<bool> ScheduleUpdateMileage()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }

            var userIdentifier = await Session.ToUserIdentifierAsync();
            try
            {
                if (!await IsGpsIntegrationConfigured())
                {
                    throw new UserFriendlyException(L("GpsIntegrationSettingsAreEmptyError"));
                }
                if (await SettingManager.GetGpsPlatformAsync() == GpsPlatform.Geotab)
                {
                    await _geotabTelematics.CheckCredentialsAsync();
                }
            }
            catch (UserFriendlyException e)
            {
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateError,
                    new MessageNotificationData($"Update mileage failed. {e.Message}"),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { userIdentifier }
                );
                return false;
            }

            await _backgroundJobManager.EnqueueAsync<UpdateMileageJob, UpdateMileageJobArgs>(new UpdateMileageJobArgs
            {
                RequestorUser = userIdentifier,
            });
            return true;
        }

        private async Task<ITruckTelematicsService> GetTruckTelematicsServiceAsync()
        {
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            return GetTruckTelematicsService(gpsPlatform);
        }

        private ITruckTelematicsService GetTruckTelematicsService(GpsPlatform gpsPlatform)
        {
            switch (gpsPlatform)
            {
                case GpsPlatform.Geotab: return _geotabTelematics;
                case GpsPlatform.Samsara: return _samsaraTelematics;
                case GpsPlatform.DtdTracker: return _dtdTrackerTelematics;
                case GpsPlatform.IntelliShift: return _intelliShiftTelematics;
                default: return null;
            }
        }

        [RemoteService(false)]
        [UnitOfWork(IsDisabled = true)]
        public async Task<(int trucksUpdated, int trucksIgnored)> UpdateMileageForCurrentTenantAsync(bool continueOnError = false)
        {
            (int trucksUpdated, int trucksIgnored) result = (0, 0);
            var truckCurrentData = new List<TruckCurrentData>();

            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return result;
            }

            var truckTelematicsService = await GetTruckTelematicsServiceAsync();
            if (truckTelematicsService == null)
            {
                return result;
            }

            if (!await truckTelematicsService.AreSettingsEmptyAsync())
            {
                try
                {
                    truckCurrentData = await truckTelematicsService.GetCurrentDataForAllTrucksAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    if (!continueOnError)
                    {
                        throw;
                    }
                }
            }

            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenantId = await Session.GetTenantIdAsync();
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var truckCodesOrUniqueIds = truckCurrentData.Select(x => x.TruckCodeOrUniqueId).Distinct().ToList();
                    var trucks = await truckTelematicsService
                        .GetTrucksQueryByTruckCodesOrUniqueIds(await _truckRepository.GetQueryAsync(), truckCodesOrUniqueIds)
                        .ToListAsync();

                    foreach (var currentData in truckCurrentData)
                    {
                        var truck = truckTelematicsService.PickTruckByTruckCodeOrUniqueId(trucks, currentData.TruckCodeOrUniqueId);
                        if (truck != null)
                        {
                            result.trucksUpdated++;
                            truck.CurrentMileage = (int)currentData.CurrentMileage;
                            truck.CurrentHours = (decimal)currentData.CurrentHours;
                            Logger.Info(
                                $"Truck with TruckCodeOrUniqueId='{currentData.TruckCodeOrUniqueId}' is updated with mileage='{(int)currentData.CurrentMileage}, hours='{currentData.CurrentHours}'.");

                            var readingDateTime = Clock.Now;
                            await _vehicleUsageRepository.InsertAsync(new VehicleUsage
                            {
                                TruckId = truck.Id,
                                ReadingDateTime = readingDateTime,
                                ReadingType = ReadingType.Miles,
                                Reading = (decimal)currentData.CurrentMileage,
                            });
                            await _vehicleUsageRepository.InsertAsync(new VehicleUsage
                            {
                                TruckId = truck.Id,
                                ReadingDateTime = readingDateTime,
                                ReadingType = ReadingType.Hours,
                                Reading = (decimal)currentData.CurrentHours,
                            });
                        }
                        else
                        {
                            result.trucksIgnored++;
                            Logger.Warn($"Truck with TruckCodeOrUniqueId='{currentData.TruckCodeOrUniqueId}' is not found in the Database.");
                        }
                    }
                }
            });

            return result;
        }

        [AbpAuthorize]
        public async Task<string> TestDtd()
        {
            return await _dtdTrackerTelematics.TestDtd();
        }

        [AbpAuthorize]
        public async Task<PagedResultDto<SelectListDto>> GetWialonDeviceTypesSelectList(GetSelectListInput input)
        {
            var query = await _wialonDeviceTypeRepository.GetQueryAsync();

            return await query
                .Select(x => new SelectListDto<WialonDeviceTypeSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new WialonDeviceTypeSelectListInfoDto
                    {
                        ServerAddress = x.ServerAddress,
                    },
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize]
        public async Task SyncWialonDeviceTypes()
        {
            await SyncWialonDeviceTypesInternal();
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        [UnitOfWork]
        public async Task SyncWialonDeviceTypesInternal()
        {
            if (_dtdTrackerTelematics.AreHostSettingsEmpty())
            {
                return;
            }
            Logger.Info("Updating Wialon Device Types");
            var apiDeviceTypes = await _dtdTrackerTelematics.GetDeviceTypes();
            var localDeviceTypes = await (await _wialonDeviceTypeRepository.GetQueryAsync()).ToListAsync();
            var localItemsToDelete = localDeviceTypes.Where(l => !apiDeviceTypes.Items.Any(a => a.Id == l.Id)).ToList();
            if (localItemsToDelete.Any())
            {
                await _wialonDeviceTypeRepository.DeleteRangeAsync(localItemsToDelete);
                Logger.Info($"Deleted {localItemsToDelete.Count} wialon device types: " + string.Join(",", localItemsToDelete.Select(x => $"{x.Id}:{x.Name}")));
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            var addedCount = 0;
            var updatedCount = 0;
            var i = 0;
            foreach (var apiItem in apiDeviceTypes.Items)
            {
                var itemIsNew = false;
                var itemWasUpdated = false;
                var localItem = localDeviceTypes.FirstOrDefault(x => x.Id == apiItem.Id);
                if (localItem == null)
                {
                    localItem = new WialonDeviceType
                    {
                        Id = apiItem.Id,
                    };
                    await _wialonDeviceTypeRepository.InsertAsync(localItem);
                    itemIsNew = true;
                    addedCount++;
                }

                var tcpPort = !string.IsNullOrEmpty(apiItem.TcpPort) && apiItem.TcpPort.All(char.IsDigit) && apiItem.TcpPort != "0" ? int.Parse(apiItem.TcpPort) : (int?)null;
                var udpPort = !string.IsNullOrEmpty(apiItem.UdpPort) && apiItem.UdpPort.All(char.IsDigit) && apiItem.UdpPort != "0" ? int.Parse(apiItem.UdpPort) : (int?)null;
                var serverAddress = (tcpPort ?? udpPort).HasValue ? apiDeviceTypes.HardwareGatewayDomain + ":" + (tcpPort ?? udpPort) : null;

                if (localItem.DeviceCategory != apiItem.DeviceCategory)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, DeviceCategory changed from {localItem.DeviceCategory} to {apiItem.DeviceCategory}");
                    }
                    localItem.DeviceCategory = apiItem.DeviceCategory;
                }
                if (localItem.Name != apiItem.Name)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, Name changed from {localItem.Name} to {apiItem.Name}");
                    }
                    localItem.Name = apiItem.Name;
                }
                if (localItem.TcpPort != tcpPort)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, TcpPort changed from {localItem.TcpPort} to {tcpPort}");
                    }
                    localItem.TcpPort = tcpPort;
                }
                if (localItem.UdpPort != udpPort)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, UdpPort changed from {localItem.UdpPort} to {udpPort}");
                    }
                    localItem.UdpPort = udpPort;
                }
                if (localItem.ServerAddress != serverAddress)
                {
                    if (!itemIsNew)
                    {
                        itemWasUpdated = true;
                        Logger.Info($"Updated wialon device type {localItem.Id}, ServerAddress changed from {localItem.ServerAddress} to {serverAddress}");
                    }
                    localItem.ServerAddress = serverAddress;
                }
                if (itemWasUpdated)
                {
                    updatedCount++;
                }
                else if (itemIsNew)
                {
                    Logger.Info($"Added wialon device type {localItem.Id}, {localItem.Name}, Server Address: {localItem.ServerAddress}");
                }

                i++;
                if (i > 300)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    i = 0;
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            Logger.Info($"Added {addedCount} wialon device types, updated {updatedCount} existing records, skipped {(apiDeviceTypes.Items.Count - addedCount - updatedCount)} unchanged records");
        }

        public async Task<SyncWithWialonResult> SyncWithWialon(SyncWithWialonInput input)
        {
            return await SyncWithWialonForCurrentTenantAsync(input);
        }

        private async Task<SyncWithWialonResult> SyncWithWialonForCurrentTenantAsync(SyncWithWialonInput input)
        {
            Logger.Info("Started sync with wialon" + (input.LocalTruckIds?.Any() == true ? ", LocalTruckIds: " + string.Join(", ", input.LocalTruckIds) : ""));
            var result = new SyncWithWialonResult();
            var truckCurrentData = new List<TruckCurrentData>();

            if (!await IsDtdTrackerConfigured())
            {
                Logger.Info("DtdTracker is not configured");
                return result;
            }

            var localTrucks = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.OfficeId != null)
                .WhereIf(input.LocalTruckIds?.Any() == true, x => input.LocalTruckIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.TruckCode,
                    x.DtdTrackerDeviceTypeId,
                    x.DtdTrackerDeviceTypeName,
                    x.DtdTrackerPassword,
                    x.DtdTrackerUniqueId,
                    x.IsActive,
                    x.EnableDriverAppGps,
                    x.VehicleCategory.IsPowered,
                }).ToListAsync();

            Logger.Info($"{localTrucks.Count} local trucks");

            var loginResult = await _dtdTrackerTelematics.LoginToApi();

            var apiTrucks = await _dtdTrackerTelematics.GetAllUnits(loginResult);
            Logger.Info($"{apiTrucks.Count} remote trucks");

            foreach (var localTruck in localTrucks)
            {
                if (localTruck.DtdTrackerUniqueId.IsNullOrEmpty())
                {
                    Logger.Info($"Skipped local truck {localTruck.TruckCode} with missing unique id");
                    continue;
                }

                if (localTruck.IsActive)
                {
                    if (!localTruck.IsPowered
                        || !localTruck.EnableDriverAppGps)
                    {
                        continue;
                    }

                    if (localTruck.DtdTrackerDeviceTypeId == null)
                    {
                        Logger.Info($"Skipped active local truck {localTruck.TruckCode} with missing device type id");
                        continue;
                    }

                    if (apiTrucks.Any(x => x.UniqueId == localTruck.DtdTrackerUniqueId))
                    {
                        Logger.Info($"Skipped local truck {localTruck.TruckCode} because a Unit with unique id {localTruck.DtdTrackerUniqueId} already exsits");
                        continue;
                    }

                    var unit = new UnitDto
                    {
                        Name = localTruck.TruckCode,
                        DeviceTypeId = localTruck.DtdTrackerDeviceTypeId.Value,
                        UniqueId = localTruck.DtdTrackerUniqueId,
                        Password = localTruck.DtdTrackerPassword,
                    };
                    await _dtdTrackerTelematics.CreateUnit(unit, loginResult);
                    apiTrucks.Add(unit);
                }
                else
                {
                    var apiTruck = apiTrucks.FirstOrDefault(x => x.UniqueId == localTruck.DtdTrackerUniqueId);
                    if (apiTruck != null)
                    {
                        await _dtdTrackerTelematics.DeleteItem(apiTruck.Id);
                        Logger.Info($"Deleted item {apiTruck.Id} because Truck {localTruck.TruckCode} with unique id {localTruck.DtdTrackerUniqueId} is inactive");
                        apiTrucks.Remove(apiTruck);
                    }
                }
            }

            await _dtdTrackerTelematics.LogoutFromApi(loginResult);

            if (input.LocalTruckIds?.Any() == true)
            {
                return result;
            }

            var apiTrucksToAddLocally = new List<Truck>();

            foreach (var apiTruck in apiTrucks)
            {
                if (!apiTruck.UniqueId.IsNullOrEmpty() && !localTrucks.Any(l => l.DtdTrackerUniqueId == apiTruck.UniqueId))
                {
                    apiTrucksToAddLocally.Add(new Truck
                    {
                        TruckCode = apiTruck.Name,
                        //VehicleCategoryId = vehicleCategory.Id,
                        CurrentHours = apiTruck.EngineHours,
                        CurrentMileage = UnitConverter.GetMiles(apiTruck.Mileage, apiTruck.MeasureUnits),
                        DtdTrackerDeviceTypeId = apiTruck.DeviceTypeId,
                        //DtdTrackerDeviceTypeName
                        DtdTrackerPassword = apiTruck.Password,
                        DtdTrackerUniqueId = apiTruck.UniqueId,
                        OfficeId = OfficeId,
                        IsActive = true,
                    });
                }
            }

            if (apiTrucksToAddLocally.Any())
            {
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                var originalMaxNumberOfTrucks = maxNumberOfTrucks;
                int currentNumberOfTrucks = localTrucks.Count(t => t.IsPowered);

                var vehicleCategory = (await _vehicleCategoryRepository.GetQueryAsync()).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).FirstOrDefault();
                if (vehicleCategory == null)
                {
                    throw new UserFriendlyException("No vehicle category was found");
                }

                var deviceTypeIds = apiTrucksToAddLocally.Select(x => x.DtdTrackerDeviceTypeId).Distinct().ToList();
                var deviceTypes = await (await _wialonDeviceTypeRepository.GetQueryAsync()).Where(x => deviceTypeIds.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.ServerAddress }).ToListAsync();

                foreach (var truck in apiTrucksToAddLocally.ToList())
                {
                    if (currentNumberOfTrucks >= maxNumberOfTrucks)
                    {
                        if (input.IncreaseNumberOfTrucksIfNeeded)
                        {
                            await CurrentUnitOfWork.SaveChangesAsync();
                            maxNumberOfTrucks += apiTrucksToAddLocally.Count;
                            Logger.Info($"Going to increase number of trucks for tenant {await Session.GetTenantIdOrNullAsync()} from {originalMaxNumberOfTrucks} to {maxNumberOfTrucks}");
                            await _truckCommonDomainService.UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(new UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput
                            {
                                NewValue = maxNumberOfTrucks,
                            });
                        }
                        else
                        {
                            result.AdditionalNumberOfTrucksRequired = apiTrucksToAddLocally.Count;
                            Logger.Info($"Not enough NumberOfTrucksFeature, needed {result.AdditionalNumberOfTrucksRequired} more");
                            break;
                        }
                    }

                    truck.VehicleCategoryId = vehicleCategory.Id;
                    var deviceType = deviceTypes.FirstOrDefault(x => x.Id == truck.DtdTrackerDeviceTypeId);
                    if (deviceType == null)
                    {
                        Logger.Error($"Wialon Device Type {truck.DtdTrackerDeviceTypeId} wasn't found in the local db for truck {truck.DtdTrackerUniqueId}");
                    }
                    else
                    {
                        truck.DtdTrackerDeviceTypeName = deviceType.Name;
                        truck.DtdTrackerServerAddress = deviceType.ServerAddress;
                    }

                    await _truckRepository.InsertAsync(truck);
                    Logger.Info($"Added local truck {truck.TruckCode} ({truck.DtdTrackerUniqueId})");
                    apiTrucksToAddLocally.Remove(truck);
                    currentNumberOfTrucks++;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task SyncWithIntelliShift()
        {
            await SyncWithIntelliShiftForCurrentTenantAsync();
        }

        private async Task SyncWithIntelliShiftForCurrentTenantAsync()
        {
            Logger.Info("Started sync with IntelliShift");

            if (!await IsIntelliShiftConfigured())
            {
                Logger.Info("IntelliShift is not configured");
                return;
            }

            var tokenLoginResult = await _intelliShiftTelematics.LoginToApiAsync();

            var localTrucks = await (await _truckRepository.GetQueryAsync())
                .ToListAsync();

            Logger.Info($"{localTrucks.Count} local trucks");

            var apiTrucks = await _intelliShiftTelematics.GetAllUnitsAsync(tokenLoginResult);

            Logger.Info($"{apiTrucks.Count} remote trucks");

            foreach (var localTruck in localTrucks)
            {
                if (localTruck.TruckCode.IsNullOrEmpty())
                {
                    Logger.Info($"Skipped local truck {localTruck.TruckCode} with missing unique id");
                    continue;
                }

                var apiTruck = apiTrucks.FirstOrDefault(p => p.Name == localTruck.TruckCode);
                if (apiTruck == null)
                {
                    Logger.Info($"Local truck {localTruck.TruckCode} is not registered in IntelliShift");
                    continue;
                }

                if (localTruck.IsActive)
                {
                    localTruck.CurrentHours = apiTruck.CumulativeHours ?? 0;
                    localTruck.CurrentMileage = apiTruck.Odometer ?? 0;
                }

                if (!localTruck.IsActive && apiTruck.IsActive)
                {
                    await _intelliShiftTelematics.UpdateUnit(apiTruck.Id,
                            tokenLoginResult, (nameof(localTruck.IsActive), localTruck.IsActive));
                }
            }

            var vehicleCategoryId = (await _vehicleCategoryRepository.GetQueryAsync())
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(x => (int?)x.Id)
                .FirstOrDefault();

            if (vehicleCategoryId == null)
            {
                throw new UserFriendlyException("No vehicle category was found");
            }

            var localTruckCodes = localTrucks.Select(p => p.TruckCode).ToList();
            var apiTrucksToAddLocally = apiTrucks
                .Where(apiTruck => !string.IsNullOrEmpty(apiTruck.Name)
                                   && !localTruckCodes.Contains(apiTruck.Name)
                                   && apiTruck.IsActive)
                .Select(apiTruck => apiTruck.ParseToTruck())
                .ToList();

            foreach (var truck in apiTrucksToAddLocally)
            {
                truck.OfficeId = OfficeId;
                truck.VehicleCategoryId = vehicleCategoryId.Value;
                await _truckRepository.InsertAsync(truck);
                Logger.Info($"Added local truck {truck.TruckCode} ({truck.Plate})");
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize]
        public async Task<bool> IsGpsIntegrationConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.DtdTracker:
                    return !(await SettingManager.GetDtdTrackerSettingsAsync()).IsEmpty();
                case GpsPlatform.Geotab:
                    return !(await SettingManager.GetGeotabSettingsAsync()).IsEmpty();
                case GpsPlatform.Samsara:
                    return !(await SettingManager.GetSamsaraSettingsAsync()).IsEmpty();
                case GpsPlatform.IntelliShift:
                    return !(await SettingManager.GetIntelliShiftSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [AbpAuthorize]
        public async Task<bool> IsDtdTrackerConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.DtdTracker:
                    return !(await SettingManager.GetDtdTrackerSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [AbpAuthorize]
        public async Task<bool> IsIntelliShiftConfigured()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                return false;
            }
            var gpsPlatform = await SettingManager.GetGpsPlatformAsync();
            switch (gpsPlatform)
            {
                case GpsPlatform.IntelliShift:
                    return !(await SettingManager.GetIntelliShiftSettingsAsync()).IsEmpty();
                default:
                    return false;
            }
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        [UnitOfWork(IsDisabled = true)]
        public async Task UpdateMileageForAllTenantsAsync()
        {
            var tenantIds = await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
            {
                return await (await _tenantRepository.GetQueryAsync())
                    .Where(t => t.IsActive)
                    .Select(x => x.Id)
                    .ToListAsync();
            });

            foreach (var tenantId in tenantIds)
            {
                using (AbpSession.Use(tenantId, null))
                {
                    if (!await FeatureChecker.IsEnabledAsync(tenantId, AppFeatures.GpsIntegrationFeature))
                    {
                        continue;
                    }

                    var gpsPlatform = (GpsPlatform)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.GpsIntegration.Platform, tenantId);
                    var truckTelematicsService = GetTruckTelematicsService(gpsPlatform);
                    if (truckTelematicsService == null)
                    {
                        continue;
                    }

                    if (await truckTelematicsService.AreSettingsEmptyAsync())
                    {
                        Logger.Warn($"There are no {gpsPlatform.GetDisplayName()} settings for TenantId={tenantId}");
                        continue;
                    }

                    Logger.Info($"Starting to update mileage for TenantId={tenantId}");
                    await UpdateMileageForCurrentTenantAsync(true);
                    Logger.Info($"Finished updating mileage for TenantId={tenantId}");
                }
            }
        }

        [UnitOfWork(IsDisabled = true)]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        [DisableConcurrentExecution(timeoutInSeconds: 5)]
        public async Task UploadTruckPositionsToWialonAsync()
        {
            var runId = Guid.NewGuid().ToShortGuid();
            var logLevel = (LogLevel)await SettingManager.GetSettingValueAsync<int>(AppSettings.Logging.UploadTruckPositionsToWialonLogLevel);
            try
            {
                if (logLevel <= LogLevel.Information)
                {
                    Logger.Info($"UploadTruckPositionsToWialon|{runId}| started at {Clock.Now:s}");
                }

                Infrastructure.Telematics.Dto.DtdTracker.TokenLoginResult loginResult = null;

                var lastUploadedTimestamp = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionTimestamp);
                var truckPositionTableClient = _azureTableManager.GetTableClient(AzureTableNames.TruckPosition);
                var truckPositionsQueryResult = truckPositionTableClient.QueryAsync<TruckPosition>(x => x.Timestamp > lastUploadedTimestamp); //(&& x.DtdTrackerUniqueId != null) or (" and DtdTrackerUniqueId ne null") throws an exception, so we'll filter the data locally for now
                var truckPositionsToUpload = new List<TruckPosition>();
                await foreach (var truckPositionResultPage in truckPositionsQueryResult.AsPages())
                {
                    truckPositionsToUpload.AddRange(truckPositionResultPage.Values);
                }

                if (!truckPositionsToUpload.Any())
                {
                    if (logLevel <= LogLevel.Information)
                    {
                        Logger.Info($"UploadTruckPositionsToWialon|{runId}| Nothing to upload after timestamp {lastUploadedTimestamp:u}");
                    }
                    return;
                }
                else
                {
                    if (logLevel <= LogLevel.Information)
                    {
                        Logger.Info($"UploadTruckPositionsToWialon|{runId}| Found {truckPositionsToUpload.Count} records to process after timestamp {lastUploadedTimestamp:u}");
                    }
                }

                try
                {
                    foreach (var group in truckPositionsToUpload.GroupBy(x => x.DtdTrackerUniqueId))
                    {
                        var dtdTrackerUniqueId = group.Key;
                        if (dtdTrackerUniqueId.IsNullOrEmpty())
                        {
                            continue;
                        }

                        loginResult ??= await _dtdTrackerTelematics.LoginToApi();

                        var unit = await _dtdTrackerTelematics.GetUnitByUniqueId(dtdTrackerUniqueId, loginResult);
                        if (unit == null)
                        {
                            if (logLevel <= LogLevel.Warning)
                            {
                                Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Unit with unique id {dtdTrackerUniqueId} wasn't found");
                            }
                            continue;
                        }

                        var messages = group.Where(x => x.Latitude.HasValue && x.Longitude.HasValue).Select(x => new GpsMessageDto
                        {
                            GpsTimestamp = x.GpsTimestamp,
                            AltitudeInMeters = x.Altitude,
                            Latitude = x.Latitude.Value,
                            Longitude = x.Longitude.Value,
                            SpeedInKMPH = (int)Math.Round((x.Speed ?? 0) * 3.6, 0), //m/s to km/h,
                            Heading = x.Heading > 0 ? (int)Math.Round(x.Heading.Value, 0) : 0,
                        }).ToList();

                        if (logLevel <= LogLevel.Information)
                        {
                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Uploading {messages.Count} messages for truck with unique id {dtdTrackerUniqueId} (unitId {unit.Id})");
                        }
                        if (logLevel <= LogLevel.Debug)
                        {
                            foreach (var message in messages)
                            {
                                Logger.Info($"UploadTruckPositionsToWialon|{runId}| Message: GpsTimestamp '{message.GpsTimestamp:u}', Lat {message.Latitude}, Lng {message.Longitude}, Spd {message.SpeedInKMPH}, Hdg {message.Heading}, Alt {message.AltitudeInMeters}");
                            }
                        }
                        await _dtdTrackerTelematics.ImportMessages(unit.Id, messages, loginResult);
                        if (logLevel <= LogLevel.Information)
                        {
                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Finished uploading for truck {dtdTrackerUniqueId} (unitId {unit.Id})");
                        }

                    }

                    var newLastTimestamp = truckPositionsToUpload.Where(x => x.Timestamp.HasValue).Max(x => x.Timestamp.Value);
                    var currentLastTimestamp = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionTimestamp);
                    if (currentLastTimestamp != lastUploadedTimestamp && currentLastTimestamp >= newLastTimestamp)
                    {
                        if (logLevel <= LogLevel.Warning)
                        {
                            Logger.Warn($"UploadTruckPositionsToWialon|{runId}| Didn't update LastUploadedTruckPositionTimestamp to {newLastTimestamp:u} because the current value is already higher or equal ({currentLastTimestamp:u})");
                        }
                    }
                    else
                    {
                        await SettingManager.ChangeSettingForApplicationAsync(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionTimestamp, newLastTimestamp.ToString("u"));
                        if (logLevel <= LogLevel.Information)
                        {
                            Logger.Info($"UploadTruckPositionsToWialon|{runId}| Changed LastUploadedTruckPositionTimestamp to {newLastTimestamp:u}");
                        }
                    }
                }
                finally
                {
                    if (loginResult != null)
                    {
                        await _dtdTrackerTelematics.LogoutFromApi(loginResult);
                    }
                }
                if (logLevel <= LogLevel.Information)
                {
                    Logger.Info($"UploadTruckPositionsToWialon|{runId}| finished at {Clock.Now:s}");
                }
            }
            catch (Exception e)
            {
                if (logLevel <= LogLevel.Error)
                {
                    Logger.Error($"UploadTruckPositionsToWialon|{runId}| failed", e);
                }
                throw;
            }
        }
    }
}
