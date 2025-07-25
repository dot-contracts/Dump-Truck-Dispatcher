using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverApp.TruckPositions.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.AzureTables;
using DispatcherWeb.TruckPositions;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DispatcherWeb.DriverApp.TruckPositions
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class TruckPositionAppService : DispatcherWebDriverAppAppServiceBase, ITruckPositionAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IAzureTableManager _azureTableManager;

        public TruckPositionAppService(
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IAzureTableManager azureTableManager
            )
        {
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _azureTableManager = azureTableManager;
        }

        [WrapResult(false)]
        public async Task Post(TruckPositionRawDto input)
        {
            if (input.Location == null)
            {
                Logger.Error("TruckPosition.Post: Location value is missing: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException("Location value is missing");
            }

            if (input.AdditionalData != null && !string.IsNullOrEmpty(input.Json))
            {
                Logger.Warn($"TruckPosition.Post: Unexpected Params fields: " + JsonConvert.SerializeObject(input));
            }

            var driver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.UserId == Session.UserId)
                .Select(x => new
                {
                    x.Id,
                    x.IsInactive,
                })
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (input.TruckId == null)
            {
                Logger.Warn("TruckPosition.Post: No TruckId was provided: " + JsonConvert.SerializeObject(input));
                return;
            }

            var truck = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.Id == input.TruckId)
                .Select(x => new
                {
                    x.DtdTrackerUniqueId,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (truck == null)
            {
                Logger.Error($"TruckPosition.Post: Truck with id {input.TruckId} wasn't found: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException($"Truck with id '{input.TruckId}' wasn't found");
            }

            if (string.IsNullOrEmpty(truck.DtdTrackerUniqueId))
            {
                Logger.Warn($"TruckPosition.Post: Truck with id {input.TruckId} doesn't have DtdTrackerUniqueId filled. Request: " + JsonConvert.SerializeObject(input));
                return;
            }

            var truckPositionTableClient = _azureTableManager.GetTableClient(AzureTableNames.TruckPosition);

            foreach (var location in input.Location)
            {
                if (location.Extras?.AdditionalData != null && !string.IsNullOrEmpty(location.Extras?.Json))
                {
                    Logger.Warn($"TruckPosition.Post: Unexpected Extras fields: " + JsonConvert.SerializeObject(location));
                }

                var truckPosition = new TruckPosition
                {
                    TruckId = input.TruckId.Value,
                    DriverId = driver?.Id,
                    TenantId = await Session.GetTenantIdAsync(),
                    CreatorUserId = Session.UserId,
                    DtdTrackerUniqueId = truck.DtdTrackerUniqueId,
                    Latitude = location.Coordinates?.Latitude,
                    Longitude = location.Coordinates?.Longitude,
                    Accuracy = location.Coordinates?.Accuracy,
                    Speed = location.Coordinates?.Speed,
                    Heading = location.Coordinates?.Heading,
                    Altitude = location.Coordinates?.Altitude,
                    ActivityType = location.Activity?.Type ?? 0,
                    ActivitiTypeRaw = location.Activity?.TypeRaw,
                    ActivityConfidence = location.Activity?.Confidence,
                    GeofenceIdentifier = location.Geofence?.Identifier,
                    GeofenceAction = location.Geofence?.Action,
                    GeofenceActionRaw = location.Geofence?.ActionRaw,
                    BatteryLevel = location.Battery?.Level,
                    BatteryIsCharging = location.Battery?.IsCharging,
                    GpsTimestamp = location.TimeStamp,
                    Uuid = location.Uuid,
                    Event = location.Event,
                    EventRaw = location.EventRaw,
                    IsMoving = location.IsMoving,
                    Odometer = location.Odometer,
                };
                await truckPositionTableClient.UpsertEntityAsync(truckPosition);
            }
        }
    }
}
