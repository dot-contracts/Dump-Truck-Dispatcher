using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Extensions;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Fulcrum.Dto;
using DispatcherWeb.Infrastructure.Telematics;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class SettingManagerExtensions
    {
        public static async Task<SmsSettingsEditDto> GetSmsSettingsAsync(this ISettingManager settingManager)
        {
            return new SmsSettingsEditDto
            {
                AccountSid = await settingManager.GetSettingValueAsync(AppSettings.Sms.AccountSid),
                AuthToken = await settingManager.GetSettingValueAsync(AppSettings.Sms.AuthToken),
                PhoneNumber = await settingManager.GetSettingValueAsync(AppSettings.Sms.PhoneNumber),
            };
        }

        public static void UpdateSmsSettings(this ISettingManager settingManager, List<SettingInfo> settingValues,
            SmsSettingsEditDto input)
        {
            settingValues.Add(new SettingInfo(AppSettings.Sms.AccountSid, input.AccountSid));
            settingValues.Add(new SettingInfo(AppSettings.Sms.AuthToken, input.AuthToken));
            settingValues.Add(new SettingInfo(AppSettings.Sms.PhoneNumber, input.PhoneNumber));
        }

        public static async Task<GeotabSettings> GetGeotabSettingsAsync(this ISettingManager settingManager)
        {
            var userName = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.User);
            var password = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Password);
            var database = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Database);
            var mapBaseUrl = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.MapBaseUrl);
            return new GeotabSettings(userName, password, database, mapBaseUrl);
        }

        public static async Task<SamsaraSettings> GetSamsaraSettingsAsync(this ISettingManager settingManager)
        {
            return new SamsaraSettings
            {
                ApiToken = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.ApiToken),
                BaseUrl = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.BaseUrl),
            };
        }

        public static async Task<IntelliShiftSettings> GetIntelliShiftSettingsAsync(this ISettingManager settingManager)
        {
            return new IntelliShiftSettings
            {
                User = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.User),
                Password = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.Password),
            };
        }

        public static async Task<DtdTrackerSettings> GetDtdTrackerSettingsAsync(this ISettingManager settingManager)
        {
            return new DtdTrackerSettings
            {
                AccountName = await settingManager.GetSettingValueAsync(AppSettings.GpsIntegration.DtdTracker.AccountName),
                AccountId = await settingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.AccountId),
                UserId = await settingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.UserId),
            };
        }

        public static async Task<GpsPlatform> GetGpsPlatformAsync(this ISettingManager settingManager)
        {
            return (GpsPlatform)await settingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.Platform);
        }

        public static async Task<FulcrumSettings> GetFulcrumSettingsAsync(this ISettingManager settingManager)
        {
            return new FulcrumSettings
            {
                UserName = await settingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.UserName),
                Password = await settingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.Password),
                CustomerNumber = await settingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.CustomerNumber),
            };
        }

        public static async Task<FulcrumSettings> GetFulcrumSettingsOrThrowAsync(this ISettingManager settingManager)
        {
            var settings = await settingManager.GetFulcrumSettingsAsync();

            if (settings.IsEmpty())
            {
                throw new UserFriendlyException("Unable to access Fulcrum. Please make sure the settings are correct.");
            }

            return settings;
        }

        public static async Task<bool> IsFulcrumIntegrationEnabledAndSetup(this ISettingManager settingManager)
        {
            var isEnabled = await settingManager.GetSettingValueAsync<bool>(AppSettings.FulcrumIntegration.IsEnabled);

            if (isEnabled)
            {
                var settings = await settingManager.GetFulcrumSettingsAsync();
                return !settings.IsEmpty();
            }

            return isEnabled;
        }

        public static async Task<IList<SelectListDto>> GetShiftSelectList(this ISettingManager settingManager)
        {
            var selectList = new List<SelectListDto>(3);
            if (!await settingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts))
            {
                return selectList;
            }
            await AddShift(AppSettings.General.ShiftName1, Shift.Shift1);
            await AddShift(AppSettings.General.ShiftName2, Shift.Shift2);
            await AddShift(AppSettings.General.ShiftName3, Shift.Shift3);

            return selectList;

            // Local functions 
            async Task AddShift(string settingName, Shift shiftValue)
            {
                string shiftName = await settingManager.GetSettingValueAsync(settingName);
                if (shiftName.IsNullOrEmpty())
                {
                    return;
                }
                selectList.Add(new SelectListDto { Id = ((int)shiftValue).ToString(), Name = shiftName });
            }
        }

        public static async Task<List<Shift?>> GetShiftsAsync(this ISettingManager settingManager)
        {
            var result = new List<Shift?>(3);
            if (!await settingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts))
            {
                result.Add(null);
                return result;
            }
            await AddShift(AppSettings.General.ShiftName1, Shift.Shift1);
            await AddShift(AppSettings.General.ShiftName2, Shift.Shift2);
            await AddShift(AppSettings.General.ShiftName3, Shift.Shift3);

            return result;

            // Local functions 
            async Task AddShift(string settingName, Shift shiftValue)
            {
                string shiftName = await settingManager.GetSettingValueAsync(settingName);
                if (shiftName.IsNullOrEmpty())
                {
                    return;
                }
                result.Add(shiftValue);
            }
        }

        public static IList<SelectListDto> WithNoShift(this IList<SelectListDto> shiftSelectList, string noShiftName)
        {
            return shiftSelectList.Union(new List<SelectListDto>
            {
                new SelectListDto
                {
                    Id = ((int)Shift.NoShift).ToString(),
                    Name = noShiftName,
                },
            }).ToList();
        }

        public static async Task<IDictionary<Shift, string>> GetShiftDictionary(this ISettingManager settingManager)
        {
            var shiftDictionary = new Dictionary<Shift, string>();
            if (!(await settingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts)))
            {
                return shiftDictionary;
            }
            await AddShift(AppSettings.General.ShiftName1, Shift.Shift1);
            await AddShift(AppSettings.General.ShiftName2, Shift.Shift2);
            await AddShift(AppSettings.General.ShiftName3, Shift.Shift3);

            return shiftDictionary;

            // Local functions 
            async Task AddShift(string settingName, Shift shiftValue)
            {
                string shiftName = await settingManager.GetSettingValueAsync(settingName);
                if (shiftName.IsNullOrEmpty())
                {
                    return;
                }
                shiftDictionary.Add(shiftValue, shiftName);
            }
        }

        public static async Task<string> GetShiftName(this ISettingManager settingManager, Shift? shift, int? tenantId = null)
        {
            if (shift == null)
            {
                return null;
            }

            switch (shift.Value)
            {
                case Shift.Shift1:
                    return await GetSettingValue(AppSettings.General.ShiftName1);
                case Shift.Shift2:
                    return await GetSettingValue(AppSettings.General.ShiftName2);
                case Shift.Shift3:
                    return await GetSettingValue(AppSettings.General.ShiftName3);
                case Shift.NoShift:
                    return "[No Shift]";
                default:
                    throw new ArgumentException($"Wrong Shift: {shift.Value}");
            }

            // Local functions
            async Task<string> GetSettingValue(string settingName) =>
                tenantId.HasValue ? await settingManager.GetSettingValueForTenantAsync(settingName, tenantId.Value) : await settingManager.GetSettingValueAsync(settingName);
        }

        public static async Task<bool> UseShifts(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
        }

        public static async Task<bool> UseShifts(this ISettingManager settingManager, int tenantId)
        {
            return await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.UseShifts, tenantId);
        }

        public static async Task<RequiredTicketEntryEnum> GetRequiredTicketEntry(this ISettingManager settingManager)
        {
            return (RequiredTicketEntryEnum)await settingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.RequiredTicketEntry);
        }

        public static async Task<DispatchVia> GetDispatchVia(this ISettingManager settingManager)
        {
            return (DispatchVia)await settingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia);
        }

        public static async Task<bool> DispatchViaSimplifiedSms(this ISettingManager settingManager)
        {
            return await settingManager.GetDispatchVia() == DispatchVia.SimplifiedSms;
        }

        public static async Task<bool> DispatchViaSimplifiedSms(this ISettingManager settingManager, int tenantId)
        {
            return (DispatchVia)await settingManager.GetSettingValueForTenantAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia, tenantId) == DispatchVia.SimplifiedSms;
        }

        public static async Task<bool> DispatchViaDriverApplication(this ISettingManager settingManager)
        {
            return await settingManager.GetDispatchVia() == DispatchVia.DriverApplication;
        }

        public static async Task<bool> DispatchViaAny(this ISettingManager settingManager)
        {
            return await settingManager.GetDispatchVia() != DispatchVia.None;
        }

        public static async Task<bool> AllowSmsMessages(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSmsMessages);
        }

        public static async Task<SendSmsOnDispatchingEnum> SendSmsOnDispatching(this ISettingManager settingManager)
        {
            return (SendSmsOnDispatchingEnum)await settingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.SendSmsOnDispatching);
        }

        public static async Task<bool> HideTicketControlsInDriverApp(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp);
        }

        [Obsolete("For new functionality use RequiredTicketEntry instead.")]
        public static async Task<bool> RequireDriversToEnterTickets(this ISettingManager settingManager)
        {
            var requiredTicketEntry = await settingManager.GetRequiredTicketEntry();
            return requiredTicketEntry.GetRequireTicketDefaultValue();
        }

        public static async Task<bool> RequireSignature(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireSignature);
        }

        public static async Task<bool> RequireTicketPhoto(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireTicketPhoto);
        }

        public static async Task<bool> DispatchesLockedToTruck(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck);
        }

        public static async Task<ShowFuelSurchargeOnInvoiceEnum> ShowFuelSurchargeOnInvoice(this ISettingManager settingManager)
        {
            if (!await settingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge))
            {
                return ShowFuelSurchargeOnInvoiceEnum.None;
            }
            return (ShowFuelSurchargeOnInvoiceEnum)await settingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ShowFuelSurchargeOnInvoice);
        }

        public static async Task<int?> GetDefaultFuelSurchargeCalculationId(this ISettingManager settingManager)
        {
            if (!await settingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge))
            {
                return null;
            }
            var result = await settingManager.GetSettingValueAsync<int>(AppSettings.Fuel.DefaultFuelSurchargeCalculationId);
            if (result == 0)
            {
                return null;
            }
            return result;
        }

        public static async Task<int?> GetDefaultFuelSurchargeCalculationIdForTenant(this ISettingManager settingManager, int tenantId)
        {
            if (!await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge, tenantId))
            {
                return null;
            }
            var result = await settingManager.GetSettingValueForTenantAsync<int>(AppSettings.Fuel.DefaultFuelSurchargeCalculationId, tenantId);
            if (result == 0)
            {
                return null;
            }
            return result;
        }

        /*
        public static async Task<bool> IsQuickbooksConnected(this ISettingManager settingManager)
        {
            if (await settingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.Quickbooks.IsConnected))
            {
                var refreshTokenExpirationDate = await settingManager.GetSettingValueAsync<DateTime>(AppSettings.Invoice.Quickbooks.RefreshTokenExpirationDate);
                if (refreshTokenExpirationDate > Clock.Now)
                {
                    return true;
                }
            }
            return false;
        }
        */

        public static async Task<bool> AllowCounterSales(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant)
                && await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser);
        }

        public static async Task<bool> AllowCounterSalesForTenant(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant);
        }

        public static async Task<bool> AutoGenerateTicketNumbers(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AutoGenerateTicketNumbers);
        }

        public static async Task<bool> DisableTicketNumberOnDriverApp(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DisableTicketNumberOnDriverApp);
        }

        public static async Task<bool> AllowLoadCountOnHourlyJobs(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs);
        }

        public static async Task<bool> AllowEditingTimeOnHourlyJobs(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowEditingTimeOnHourlyJobs);
        }

        public static async Task<bool> AllowMultipleDispatchesToBeInProgressAtTheSameTime(this ISettingManager settingManager)
        {
            return await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowMultipleDispatchesToBeInProgressAtTheSameTime);
        }

        public static async Task<UnitOfMeasureBaseEnum> GetHaulRateCalculationUomBaseId(this ISettingManager settingManager, bool forCod)
        {
            return (UnitOfMeasureBaseEnum)await settingManager.GetSettingValueAsync<int>(
                forCod
                    ? AppSettings.HaulZone.HaulRateCalculation.BaseUomIdForCod
                    : AppSettings.HaulZone.HaulRateCalculation.BaseUomId
            );
        }
    }
}
