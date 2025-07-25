using Abp.Configuration;

namespace DispatcherWeb.Dashboard
{
    public class DashboardSetting
    {
        public string SettingName { get; set; }
        public string SettingLocalizableLabel { get; set; }
        public string PermissionName { get; set; }
        public string FeatureName { get; set; }

        public SettingDefinition SettingDefinition =>
            new SettingDefinition(SettingName, "true", scopes: SettingScopes.User, isVisibleToClients: true);


        public T CopyTo<T>(T other) where T : DashboardSetting
        {
            other.SettingName = SettingName;
            other.SettingLocalizableLabel = SettingLocalizableLabel;
            other.PermissionName = PermissionName;
            other.FeatureName = FeatureName;
            return other;
        }
    }
}
