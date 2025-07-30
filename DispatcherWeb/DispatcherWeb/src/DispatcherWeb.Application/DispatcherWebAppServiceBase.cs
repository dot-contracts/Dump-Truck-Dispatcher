using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Shifts;

namespace DispatcherWeb
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class DispatcherWebAppServiceBase : ApplicationService
    {
        protected DispatcherWebAppServiceBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected async Task<string> GetTimezone()
        {
            return await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
        }

        protected async Task<string> GetTimezone(int? tenantId, long userId)
        {
            return await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone, tenantId, userId);
        }

        protected async Task<bool> CheckUseShiftSettingCorrespondsInput(Shift? shift)
        {
            if (shift == null)
            {
                return false;
            }

            return await CheckUseShiftSettingCorrespondsInput(new[] { shift });
        }

        protected async Task<bool> CheckUseShiftSettingCorrespondsInput(Shift[] shifts)
        {
            if (shifts == null || shifts.Length == 0)
            {
                return false;
            }

            var useShiftSetting = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShift);
            var hasShift = shifts.Any(s => s != null);

            return useShiftSetting == hasShift;
        }
    }
}
