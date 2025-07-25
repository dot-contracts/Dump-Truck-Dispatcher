using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Localization;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.Trucks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = Castle.Core.Logging.ILogger;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class DtdTrackerTelematics : TruckTelematicsServiceBase, IDtdTrackerTelematics, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IConfigurationRoot _appConfiguration;

        public ILogger Logger { get; set; }

        public DtdTrackerTelematics(
            ISettingManager settingManager,
            ILocalizationManager localizationManager,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _settingManager = settingManager;
            _localizationManager = localizationManager;
            _appConfiguration = configurationAccessor.Configuration;
        }

        private string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        private async Task<TResult> PostToApi<TResult>(TokenLoginResult loginResult, string command, object parameters, HttpContent httpContent = null) where TResult : IWialonResult
        {
            var stringApiResult = await PostToApi(loginResult, command, parameters, httpContent);

            TResult result;
            try
            {
                result = JsonConvert.DeserializeObject<TResult>(stringApiResult);
            }
            catch (JsonException)
            {
                Logger.Error("Failed to deserialize wialon result: " + stringApiResult);
                throw;
            }

            if (result.ErrorCode > 0)
            {
                //login error:
                //{"error":8, "reason":"INVALID_AUTH_TOKEN"}
                //items search error:
                //{"error":1}
                //{"error":4, "reason":"VALIDATE_PARAMS_ERROR: {spec: {itemsType: text, propName: text, propValueMask: text, sortType: text}, force: uint, flags: long, from: uint, to: uint}"}
                throw new ApplicationException($"Received error code {result.ErrorCode} {result.ErrorReason} from wialon, {stringApiResult}");
            }

            return result;
        }

        private async Task<string> PostToApi(TokenLoginResult loginResult, string command, object parameters, HttpContent httpContent = null)
        {
            var baseUrl = _appConfiguration["DtdTracker:BaseUrl"];
            if (baseUrl.IsNullOrEmpty())
            {
                throw new ApplicationException("DTDTracker host settings are missing");
            }

            var needToLogout = false;
            if (loginResult == null && !WialonCommands.AnonymousCommands.Contains(command))
            {
                needToLogout = true;
                loginResult = await LoginToApi();
            }

            var emptyBody = httpContent != null ? null : new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseUrl);

                var optionalSessionParam = loginResult == null ? "" : $@"sid={loginResult.SessionId}&";
                var jsonParams = JsonConvert.SerializeObject(parameters);
                var rawApiResult = await httpClient.PostAsync($@"/wialon/ajax.html?{optionalSessionParam}svc={command}&params={jsonParams}", httpContent ?? emptyBody);
                if (rawApiResult.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UserFriendlyException(L("DtdTrackerAuthError"));
                }
                rawApiResult.EnsureSuccessStatusCode();
                var stringApiResult = await rawApiResult.Content.ReadAsStringAsync();

                if (emptyBody != null)
                {
                    emptyBody.Dispose();
                }

                if (needToLogout)
                {
                    await LogoutFromApi(loginResult);
                }

                return stringApiResult;
            }
        }

        public async Task<TokenLoginResult> LoginToApi(string accessToken = null)
        {
            if (accessToken == null)
            {
                accessToken = _appConfiguration["DtdTracker:AccessToken"];
                if (accessToken.IsNullOrEmpty())
                {
                    throw new ApplicationException("DTDTracker host settings are missing");
                }
            }
            return await PostToApi<TokenLoginResult>(null, WialonCommands.TokenLogin, new
            {
                token = accessToken,
                fl = 3, //basic info + user info
            });
        }

        public async Task<WialonResult> LogoutFromApi(TokenLoginResult loginResult)
        {
            try
            {
                var logoutResult = await PostToApi<WialonResult>(loginResult, WialonCommands.Logout, new { });
                return logoutResult;
            }
            catch (Exception e)
            {
                Logger.Error("Error during wialon logout", e);
                return null;
            }
        }

        public bool AreHostSettingsEmpty()
        {
            var baseUrl = _appConfiguration["DtdTracker:BaseUrl"];
            var accessToken = _appConfiguration["DtdTracker:AccessToken"];
            return baseUrl.IsNullOrEmpty()
                || accessToken.IsNullOrEmpty();
        }

        public override async Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync()
        {
            if (AreHostSettingsEmpty())
            {
                Logger.Warn("DtdTracker host settings are missing");
                return new List<TruckCurrentData>();
            }
            var settings = await _settingManager.GetDtdTrackerSettingsAsync();
            if (settings.IsEmpty())
            {
                return new List<TruckCurrentData>();
            }

            var loginResult = await LoginToApi();

            var apiUnits = await PostToApi<ItemsSearchResult<UnitDto>>(loginResult, WialonCommands.SearchItems, new
            {
                spec = new
                {
                    itemsType = "avl_unit",
                    propName = "sys_billing_account_guid",
                    propValueMask = settings.AccountId.ToString(),
                    sortType = "sys_name",
                    //"propType":<text>,
                    //"or_logic":<bool>
                },
                force = 1,
                flags = 8453, //0x00000004 + 0x00000001 + 0x00000100 + 0x00002000 = 0x00002105 = 8453
                from = 0,
                to = 0,
            });

            var result = new List<TruckCurrentData>();
            foreach (var unit in apiUnits.Items)
            {
                result.Add(new TruckCurrentData
                {
                    TruckCodeOrUniqueId = unit.UniqueId,
                    CurrentHours = Convert.ToDouble(unit.EngineHours),
                    CurrentMileage = UnitConverter.GetMiles(Convert.ToDouble(unit.Mileage), unit.MeasureUnits),
                });
            }

            await LogoutFromApi(loginResult);

            return result;
        }

        public async Task<WialonDeviceTypesResult> GetDeviceTypes()
        {
            var loginResult = await LoginToApi();

            var items = await PostToApi<WialonListResult<DeviceTypeDto>>(loginResult, WialonCommands.GetDeviceTypes, new
            {
                includeType = 1,
            });

            await LogoutFromApi(loginResult);

            return new WialonDeviceTypesResult
            {
                Items = items,
                HardwareGatewayDomain = loginResult.HardwareGatewayDomain,
            };
        }

        private async Task<DtdTrackerSettings> GetDtdSettingsOrThrow()
        {
            var settings = await _settingManager.GetDtdTrackerSettingsAsync();
            if (settings.IsEmpty())
            {
                throw new UserFriendlyException(L("MissingDtdTrackerSettings"));
            }
            return settings;
        }

        public async Task<string> TestDtd()
        {
            await Task.CompletedTask;
            //var settings = await GetDtdSettingsOrThrow();
            //var loginResult = await LoginToApi();

            //await LogoutFromApi(loginResult);
            return "";
        }

        public async Task CreateUnit(UnitDto unit, TokenLoginResult loginResult = null)
        {
            var needToLogout = false;
            if (loginResult == null)
            {
                needToLogout = true;
                loginResult = await LoginToApi();
            }

            var settings = await GetDtdSettingsOrThrow();

            var createUnitResult = await PostToApi<ItemSearchResult<UnitDto>>(loginResult, WialonCommands.CreateUnit, new
            {
                creatorId = settings.UserId,
                name = (unit.Name?.Trim() ?? "").PadLeft(4, '0'),
                hwTypeId = unit.DeviceTypeId,
                dataFlags = 1,
            });
            //success:
            //{"item":{"nm":"QA100","cls":2,"id":400500575,"mu":1,"uacl":880333093887},"flags":1}

            var itemId = createUnitResult.Item.Id;
            unit.Id = itemId;
            Logger.Info($"Created wialon unit {itemId}:{unit.Name}");

            await PostToApi<WialonResult>(loginResult, WialonCommands.UpdateDeviceType, new
            {
                itemId = itemId,
                deviceTypeId = unit.DeviceTypeId,
                uniqueId = unit.UniqueId,
            });
            //success:
            //{"uid":"dispatcherqaQA100","hw":400000192,"hwd":0}

            await PostToApi<WialonResult>(loginResult, WialonCommands.UpdateUnitPassword, new
            {
                itemId = itemId,
                accessPassword = unit.Password,
            });
            //success:
            //{"psw":"123qwe"}

            await PostToApi<WialonResult>(loginResult, WialonCommands.UpdateUnitDriverRankSettings, new
            {
                itemId = itemId,
                driveRank = new
                {
                    acceleration = new object[]
                    {
                        new {flags = 2, min_value = 0.25, name = "Acceleration: extreme", penalties = 2000 },
                        new {flags = 2, max_value = 0.25, min_value = 0.16, name = "Acceleration: medium", penalties = 500},
                    },
                    brake = new object[]
                    {
                        new {flags = 2, min_value = 0.25, name = "Brake: extreme", penalties = 2000 },
                        new {flags = 2, max_value = 0.25, min_value = 0.16, name = "Brake: medium", penalties = 500},
                    },
                    speeding = new object[]
                    {
                        new { flags = 2, max_duration = 30, min_duration = 10, min_value = 19.3, name = "Speeding: extreme", penalties = 5000},
                        new { flags = 2, max_value = 19.3, min_duration = 10, min_value = 13, name = "Speeding: medium", penalties = 2000},
                        new { flags = 2, max_value = 12.4, min_duration = 10, min_value = 0.6, name = "Speeding: mild", penalties = 100},
                    },
                    turn = new object[]
                    {
                        new {flags = 2, min_value = 0.25, name = "Turn: extreme", penalties = 500},
                        new {flags = 2, max_value = 0.25, min_value = 0.16, name = "Turn: medium", penalties = 500},
                    },
                    global = new { accel_mode = 0 },
                },
            });

            if (needToLogout)
            {
                await LogoutFromApi(loginResult);
            }
        }

        public async Task DeleteItem(int itemId, TokenLoginResult loginResult = null)
        {
            var needToLogout = false;
            if (loginResult == null)
            {
                needToLogout = true;
                loginResult = await LoginToApi();
            }

            await PostToApi<WialonResult>(loginResult, WialonCommands.DeleteItem, new
            {
                itemId = itemId,
            });
            //success:
            //{}
            Logger.Info($"Deleted wialon item {itemId}");

            if (needToLogout)
            {
                await LogoutFromApi(loginResult);
            }
        }

        /// <summary>
        /// This does not add a filter by tenant / DTD account
        /// </summary>
        public async Task<UnitDto> GetUnitByUniqueId(string uniqueId, TokenLoginResult loginResult)
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                return null;
            }

            var apiUnits = await PostToApi<ItemsSearchResult<UnitDto>>(loginResult, WialonCommands.SearchItems, new
            {
                spec = new
                {
                    itemsType = "avl_unit",
                    propName = "sys_unique_id",
                    propValueMask = uniqueId,
                    sortType = "sys_name",
                    //"propType":<text>,
                    //"or_logic":<bool>
                },
                force = 1,
                flags = 8453, //0x00000004 + 0x00000001 + 0x00000100 + 0x00002000 = 0x00002105 = 8453
                from = 0,
                to = 0,
            });

            return apiUnits.Items.FirstOrDefault();
        }

        public async Task<List<UnitDto>> GetAllUnits(TokenLoginResult loginResult = null)
        {
            var needToLogout = false;
            if (loginResult == null)
            {
                needToLogout = true;
                loginResult = await LoginToApi();
            }

            var settings = await GetDtdSettingsOrThrow();

            var apiUnits = await PostToApi<ItemsSearchResult<UnitDto>>(loginResult, WialonCommands.SearchItems, new
            {
                spec = new
                {
                    itemsType = "avl_unit",
                    propName = "sys_billing_account_guid",
                    propValueMask = settings.AccountId.ToString(),
                    sortType = "sys_name",
                    //"propType":<text>,
                    //"or_logic":<bool>
                },
                force = 1,
                flags = 8453, //0x00000004 + 0x00000001 + 0x00000100 + 0x00002000 = 0x00002105 = 8453
                from = 0,
                to = 0,
            });

            if (needToLogout)
            {
                await LogoutFromApi(loginResult);
            }

            return apiUnits.Items;
        }

        public async Task<DtdTrackerSettings> GetAccountDetailsFromAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var newUserLoginResult = await LoginToApi(accessToken);
            await LogoutFromApi(newUserLoginResult);

            var adminLoginResult = await LoginToApi();
            var accountDetails = await PostToApi<ItemSearchResult<AccountDto>>(adminLoginResult, WialonCommands.SearchItem, new
            {
                id = newUserLoginResult.User.AccountId,
                flags = 1,
            });
            await LogoutFromApi(adminLoginResult);

            return new DtdTrackerSettings
            {
                UserId = newUserLoginResult.User.Id,
                AccountId = newUserLoginResult.User.AccountId,
                AccountName = accountDetails.Item.Name,
            };
        }

        public async Task ImportMessages(int unitId, List<GpsMessageDto> messages, TokenLoginResult loginResult)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var logLevel = (LogLevel)await _settingManager.GetSettingValueAsync<int>(AppSettings.Logging.UploadTruckPositionsToWialonLogLevel);
            if (logLevel <= LogLevel.Information)
            {
                Logger.Info($"Uploading messages for wialon unit {unitId} ({messages.Count} messages)");
            }


            var fileToUpload = string.Join("\r\n", messages.Select(x => x.ToString()));

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileToUpload));
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multipartFormContent.Add(byteContent, name: "messages_filter_import_file", fileName: "import file.wln"); //The space in the filename is important, wialon doesn't parse a non-quoted filename parameter

                await PostToApi<WialonResult>(loginResult, WialonCommands.ImportMessages, new
                {
                    itemId = unitId,
                }, multipartFormContent);
            }

            if (logLevel <= LogLevel.Information)
            {
                Logger.Info($"Uploading messages for wialon unit {unitId} ({messages.Count} messages) completed");
            }
        }

        public override IQueryable<Truck> GetTrucksQueryByTruckCodesOrUniqueIds(IQueryable<Truck> query, List<string> truckCodesOrUniqueIds)
        {
            return query.Where(t => truckCodesOrUniqueIds.Contains(t.DtdTrackerUniqueId));
        }

        public override Truck PickTruckByTruckCodeOrUniqueId(List<Truck> trucks, string truckCodeOrUniqueId)
        {
            return trucks.FirstOrDefault(t => t.DtdTrackerUniqueId == truckCodeOrUniqueId);
        }

        public async Task<DtdTrackerSettings> GetSettingsAsync()
        {
            return await _settingManager.GetDtdTrackerSettingsAsync();
        }

        public override async Task<bool> AreSettingsEmptyAsync()
        {
            return (await GetSettingsAsync()).IsEmpty();
        }
    }
}
