using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Localization;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Utilities;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class GeotabTelematics : TruckTelematicsServiceBase, IGeotabTelematics, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly ILocalizationManager _localizationManager;

        public ILogger Logger { get; set; }

        public GeotabTelematics(
            ISettingManager settingManager,
            ILocalizationManager localizationManager
        )
        {
            _settingManager = settingManager;
            _localizationManager = localizationManager;
        }

        private async Task<API> GetApiAsync()
        {
            var geotabSettings = await _settingManager.GetGeotabSettingsAsync();
            if (geotabSettings.IsEmpty())
            {
                throw new UserFriendlyException(L("GeotabSettingsAreEmptyError"));
            }
            string server;
            try
            {
                server = geotabSettings.MapBaseUrl.IsNullOrEmpty() ? null : new Uri(geotabSettings.MapBaseUrl).Host;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException(L("GeotabSettingsUrlParsingError"));
            }
            var api = new API(geotabSettings.UserName, geotabSettings.Password, null, geotabSettings.Database, server);

            try
            {
                await api.AuthenticateAsync();
            }
            catch (InvalidUserException e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException(L("GeotabSettingsWrongCredentialsError"));
            }
            catch (DbUnavailableException e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException(L("GeotabSettingsWrongDb"));
            }
            catch (OverLimitException e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException(e.Message);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException(L("GeotabError"));
            }

            return api;
        }

        public async Task CheckCredentialsAsync()
        {
            await GetApiAsync();
        }

        private string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }


        public override async Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync()
        {
            Logger.Debug("Calling Get for Device");
            var api = await GetApiAsync();
            var devices = await api.CallAsync<IList<Device>>("Get", typeof(Device));
            Logger.Debug($"Returned {devices.Count} devices");
            var result = new List<TruckCurrentData>();
            foreach (var device in devices)
            {
                result.Add(await GetTruckCurrentDataAsync(api, device));
            }
            return result;
        }

        private async Task<TruckCurrentData> GetTruckCurrentDataAsync(API api, Device device)
        {
            Logger.Debug($"Get current data for Id='{device.Id}', TruckCode='{device.Name}'");
            // Search for status data based on the current device and the odometer reading
            //var statusDataSearch = new StatusDataSearch
            //{
            //    DeviceSearch = new DeviceSearch(device.Id),
            //    DiagnosticSearch = new DiagnosticSearch(KnownId.DiagnosticOdometerAdjustmentId),
            //    FromDate = DateTime.MaxValue
            //};

            var calls = new object[]
            {
                GetStatusDataSearchObject(device.Id, KnownId.DiagnosticOdometerAdjustmentId),
                GetStatusDataSearchObject(device.Id, KnownId.DiagnosticEngineHoursAdjustmentId),
            };
            // Retrieve the odometer status data
            //IList<StatusData> statusData =
            //    await Api.CallAsync<IList<StatusData>>("Get", typeof(StatusData), new {search = statusDataSearch});
            var results = await api.MultiCallAsync(calls);
            var odometerStatusData = (IList<StatusData>)results[0];
            var engineHoursStatusData = (IList<StatusData>)results[1];

            var odometerReading = UnitConverter.ConvertMetresToMiles(odometerStatusData[0].Data ?? 0);
            var engineHoursReading = UnitConverter.ConvertSecondsToHours(engineHoursStatusData[0].Data ?? 0);
            Logger.Debug($"Odometer: {odometerReading}, Hours: {engineHoursReading}");

            return new TruckCurrentData
            {
                TruckCodeOrUniqueId = device.Name,
                CurrentMileage = odometerReading,
                CurrentHours = engineHoursReading,
            };
        }

        private object[] GetStatusDataSearchObject(Id deviceId, Id knowId)
        {
            var statusDataSearch = new StatusDataSearch
            {
                DeviceSearch = new DeviceSearch(deviceId),
                DiagnosticSearch = new DiagnosticSearch(knowId),
                FromDate = DateTime.MaxValue,
            };
            return new object[] { "Get", typeof(StatusData), new { search = statusDataSearch }, typeof(List<StatusData>) };
        }

        public async Task<string[]> GetDeviceIdsByTruckCodesAsync(string[] truckCodes)
        {
            if (truckCodes.Length == 0)
            {
                return new string[0];
            }
            Logger.Debug("Calling Get for Device");
            var api = await GetApiAsync();
            var devices = await api.CallAsync<IList<Device>>("Get", typeof(Device));
            Logger.Debug($"Returned {devices.Count} devices");
            return devices
                .Where(d => truckCodes.Contains(d.Name))
                .Select(d => d.Id.ToString())
                .ToArray();
        }

        public async Task<GeotabSettings> GetSettingsAsync()
        {
            return await _settingManager.GetGeotabSettingsAsync();
        }

        public override async Task<bool> AreSettingsEmptyAsync()
        {
            return (await GetSettingsAsync()).IsEmpty();
        }

    }
}
