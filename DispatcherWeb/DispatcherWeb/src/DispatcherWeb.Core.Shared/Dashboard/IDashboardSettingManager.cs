using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Dashboard
{
    public interface IDashboardSettingManager
    {
        Task<List<DashboardUserSetting>> GetDashboardUserSettingsAsync();
        Task<bool> IsAnyEnabledAsync(DashboardSetting[] settings);
        Task<bool> IsEnabledAsync(DashboardSetting setting);
        Task SetDashboardUserSettingAsync(string settingName, bool isEnabled);
        Task<bool> ShouldShowSettingAsync(DashboardSetting dashboardSetting);
        Task<bool> ShouldShowSettingAsync(string dashboardSettingName);
    }
}
