using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Localization;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;
using DispatcherWeb.Infrastructure.Utilities;
using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class IntelliShiftTelematics : TruckTelematicsServiceBase, IIntelliShiftTelematics, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly ILocalizationManager _localizationManager;

        public ILogger Logger { get; set; }

        public IntelliShiftTelematics(
            ISettingManager settingManager,
            ILocalizationManager localizationManager
        )
        {
            _settingManager = settingManager;
            _localizationManager = localizationManager;
        }

        public async Task<TokenLoginResult> LoginToApiAsync()
        {
            var settings = await _settingManager.GetIntelliShiftSettingsAsync();

            if (settings.IsEmpty())
            {
                throw new ApplicationException("IntelliShift host credential settings are missing");
            }

            var apiAuthUrl = await _settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.ApiAuthUrl);

            using var httpClient = new HttpClient();
            var apiResponse = await httpClient.PostAsJsonAsync(apiAuthUrl, new
            {
                email = settings.User,
                password = settings.Password,
            });

            if (apiResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new UserFriendlyException(L("IntelliShiftAuthError"));
            }

            apiResponse.EnsureSuccessStatusCode();
            var jsonApiResult = await apiResponse.Content.ReadAsStringAsync();
            var apiAuthResult = JsonConvert.DeserializeObject<TokenLoginResult>(jsonApiResult);

            if (!string.IsNullOrEmpty(apiAuthResult.Error)
                || !string.IsNullOrEmpty(apiAuthResult.ErrorDescription))
            {
                var innerException = new Exception($"{apiAuthResult.Error}: {apiAuthResult.ErrorDescription}");
                Logger.Error(innerException.ToString(), innerException);
                throw new UserFriendlyException(L("IntelliShiftAuthError"), innerException);
            }
            return apiAuthResult;
        }

        public async Task<List<TruckUnitDto>> GetAllUnitsAsync(TokenLoginResult tokenLoginResult = null)
        {
            var truckUnits = new List<TruckUnitDto>();

            if (await _settingManager.GetGpsPlatformAsync() != GpsPlatform.IntelliShift)
            {
                return truckUnits;
            }

            tokenLoginResult ??= await LoginToApiAsync();

            var currentPage = 1;
            var pageSize = 250;
            TruckUnitsPage truckUnitsPage;

            var apiBaseUrl = await _settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.BaseUrl);

            using var httpClient = new HttpClient();
            do
            {
                try
                {
                    await Task.Delay(10); //API has a limit of max 150 requests per second (one per 7ms)

                    var requestUrl = $"{apiBaseUrl}/assets/vehicles?ShowInactive=true&PageNumber={currentPage}&PageSize={pageSize}&SortBy=Id&SortDirection=ASC&Filter=";
                    using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUrl));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLoginResult.AccessToken);
                    var rawApiResult = await httpClient.SendAsync(request);
                    if (rawApiResult.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UserFriendlyException(L("IntelliShiftAuthError"));
                    }
                    rawApiResult.EnsureSuccessStatusCode();
                    var jsonApiResult = await rawApiResult.Content.ReadAsStringAsync();

                    truckUnitsPage = JsonConvert.DeserializeObject<TruckUnitsPage>(jsonApiResult);
                    truckUnits.AddRange(truckUnitsPage.TruckUnitsCollection);
                    currentPage++;
                }
                catch (HttpRequestException e)
                {
                    Logger.Error(e.ToString(), e);
                    throw new UserFriendlyException(L("IntelliShiftError"));
                }
            }
            while (truckUnitsPage.HasMorePages);
            return truckUnits;
        }

        public override async Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync()
        {
            var tokenLoginResult = await LoginToApiAsync();
            var truckUnits = await GetAllUnitsAsync(tokenLoginResult);
            var results = new List<TruckCurrentData>();

            truckUnits.ForEach(truckUnit =>
            {
                results.Add(new TruckCurrentData
                {
                    TruckCodeOrUniqueId = truckUnit.Name,
                    CurrentHours = (double)(truckUnit.CumulativeHours ?? 0),
                    CurrentMileage = UnitConverter.ConvertMetresToMiles((double)(truckUnit.Odometer ?? 0)),
                });
            });
            return results;
        }

        public async Task<bool> UpdateUnit(int remoteVehicleId,
                                            TokenLoginResult tokenLoginResult = null,
                                            params (string PropertyName, object PropertyValue)[] fieldsToUpdate)
        {
            tokenLoginResult ??= await LoginToApiAsync();

            var fieldsDic = new Dictionary<string, object>();
            Array.ForEach(fieldsToUpdate, field =>
            {
                var jsonAttribute = TruckUnitDto.GetJsonPropertyAttribute(field.PropertyName);
                fieldsDic.Add(jsonAttribute, field.PropertyValue);
            });
            var fieldsJson = JsonConvert.SerializeObject(fieldsDic);
            var apiBaseUrl = await _settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.BaseUrl);

            var requestUrl = $"{apiBaseUrl}/assets/vehicles/{remoteVehicleId}";
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Patch, new Uri(requestUrl))
            {
                Content = new StringContent(fieldsJson, Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLoginResult.AccessToken);
            var rawApiResult = await httpClient.SendAsync(request);
            if (rawApiResult.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UserFriendlyException(L("IntelliShiftAuthError"));
            }
            rawApiResult.EnsureSuccessStatusCode();
            return rawApiResult.IsSuccessStatusCode;
        }

        private string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        public async Task<IntelliShiftSettings> GetSettingsAsync()
        {
            return await _settingManager.GetIntelliShiftSettingsAsync();
        }

        public override async Task<bool> AreSettingsEmptyAsync()
        {
            return (await GetSettingsAsync()).IsEmpty();
        }
    }
}
