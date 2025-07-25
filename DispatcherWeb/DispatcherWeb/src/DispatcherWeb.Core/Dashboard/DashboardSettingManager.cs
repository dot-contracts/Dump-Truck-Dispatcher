using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Runtime.Session;

namespace DispatcherWeb.Dashboard
{
    public class DashboardSettingManager : IDashboardSettingManager, ITransientDependency
    {
        public DashboardSettingManager(
            ISettingManager settingManager,
            IPermissionChecker permissionChecker,
            IFeatureChecker featureChecker,
            IAbpSession abpSession
            )
        {
            SettingManager = settingManager;
            PermissionChecker = permissionChecker;
            FeatureChecker = featureChecker;
            AbpSession = abpSession;
        }

        public ISettingManager SettingManager { get; }
        public IPermissionChecker PermissionChecker { get; }
        public IFeatureChecker FeatureChecker { get; }
        public IAbpSession AbpSession { get; }

        public async Task<bool> ShouldShowSettingAsync(string dashboardSettingName)
        {
            var setting = DashboardSettings.All.FirstOrDefault(x => x.SettingName == dashboardSettingName);
            if (setting == null)
            {
                return false;
            }
            return await ShouldShowSettingAsync(setting);
        }

        public async Task<bool> ShouldShowSettingAsync(DashboardSetting setting)
        {
            if (!string.IsNullOrEmpty(setting.FeatureName)
                && !await FeatureChecker.IsEnabledAsync(setting.FeatureName))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(setting.PermissionName)
                && !await PermissionChecker.IsGrantedAsync(setting.PermissionName))
            {
                return false;
            }

            return true;
        }

        public async Task<List<DashboardUserSetting>> GetDashboardUserSettingsAsync()
        {
            var result = new List<DashboardUserSetting>();
            foreach (var setting in DashboardSettings.All)
            {
                if (!await ShouldShowSettingAsync(setting))
                {
                    continue;
                }

                result.Add(setting.CopyTo(new DashboardUserSetting
                {
                    IsEnabled = await SettingManager.GetSettingValueAsync<bool>(setting.SettingName),
                }));
            }
            return result;
        }

        public async Task SetDashboardUserSettingAsync(string settingName, bool isEnabled)
        {
            if (!await ShouldShowSettingAsync(settingName))
            {
                return;
            }

            await SettingManager.ChangeSettingForUserAsync(await AbpSession.ToUserIdentifierAsync(), settingName, isEnabled.ToString().ToLowerInvariant());
        }

        public async Task<bool> IsAnyEnabledAsync(DashboardSetting[] settings)
        {
            foreach (var setting in settings)
            {
                if (await IsEnabledAsync(setting))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> IsEnabledAsync(DashboardSetting setting)
        {
            if (!await ShouldShowSettingAsync(setting))
            {
                return false;
            }

            return await SettingManager.GetSettingValueAsync<bool>(setting.SettingName);
        }
    }
}
