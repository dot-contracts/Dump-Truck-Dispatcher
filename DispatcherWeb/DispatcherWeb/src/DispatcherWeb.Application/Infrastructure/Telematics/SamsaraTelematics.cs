using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Localization;
using Abp.UI;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.Samsara;
using DispatcherWeb.Infrastructure.Utilities;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class SamsaraTelematics : TruckTelematicsServiceBase, ISamsaraTelematics, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly ILocalizationManager _localizationManager;

        public ILogger Logger { get; set; }

        public SamsaraTelematics(
            ISettingManager settingManager,
            ILocalizationManager localizationManager
        )
        {
            _settingManager = settingManager;
            _localizationManager = localizationManager;
        }

        private string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        public override async Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync()
        {
            var settings = await _settingManager.GetSamsaraSettingsAsync();
            if (settings.IsEmpty() || await _settingManager.GetGpsPlatformAsync() != GpsPlatform.Samsara)
            {
                return new List<TruckCurrentData>();
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(settings.BaseUrl);
                httpClient.SetBearerToken(settings.ApiToken);

                string endCursor = "";
                var stats = new List<VehicleStats>();
                VehiclesStatsResult apiResult;
                do
                {
                    try
                    {
                        await Task.Delay(10); //API has a limit of max 150 requests per second (one per 7ms)
                        var rawApiResult = await httpClient.GetAsync($"/fleet/vehicles/stats?types=obdOdometerMeters,obdEngineSeconds,gpsOdometerMeters&after={endCursor}");
                        if (rawApiResult.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            throw new UserFriendlyException(L("SamsaraAuthError"));
                        }
                        rawApiResult.EnsureSuccessStatusCode();
                        var stringApiResult = await rawApiResult.Content.ReadAsStringAsync();
                        apiResult = JsonConvert.DeserializeObject<VehiclesStatsResult>(stringApiResult);
                        endCursor = apiResult.Pagination.EndCursor;
                        stats.AddRange(apiResult.Data);
                    }
                    catch (HttpRequestException e)
                    {
                        Logger.LogError(e, e.ToString());
                        throw new UserFriendlyException(L("SamsaraError"));
                    }
                }
                while (apiResult.Pagination.HasNextPage);

                var result = new List<TruckCurrentData>();
                foreach (var statGroup in stats.GroupBy(x => x.Name))
                {
                    var statToUse = statGroup.First();
                    if (statGroup.Count() > 1)
                    {
                        foreach (var stat in statGroup)
                        {
                            if (stat.ObdOdometerMeters?.Time > statToUse.ObdOdometerMeters?.Time
                                || stat.ObdEngineSeconds?.Time > statToUse.ObdEngineSeconds?.Time
                                || stat.ObdOdometerMeters == null && stat.GpsOdometerMeters?.Time > statToUse.GpsOdometerMeters?.Time)
                            {
                                statToUse = stat;
                            }
                        }
                    }
                    result.Add(new TruckCurrentData
                    {
                        TruckCodeOrUniqueId = statToUse.Name,
                        CurrentHours = UnitConverter.ConvertSecondsToHours(statToUse.ObdEngineSeconds?.Value ?? 0),
                        CurrentMileage = UnitConverter.ConvertMetresToMiles(statToUse.ObdOdometerMeters?.Value ?? statToUse.GpsOdometerMeters?.Value ?? 0),
                    });
                }
                return result;
            }
        }

        public async Task<SamsaraSettings> GetSettingsAsync()
        {
            return await _settingManager.GetSamsaraSettingsAsync();
        }

        public override async Task<bool> AreSettingsEmptyAsync()
        {
            return (await GetSettingsAsync()).IsEmpty();
        }
    }
}
