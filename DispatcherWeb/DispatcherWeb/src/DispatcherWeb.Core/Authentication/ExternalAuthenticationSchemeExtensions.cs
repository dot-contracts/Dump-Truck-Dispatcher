using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.Authentication
{
    public static class ExternalAuthenticationSchemeExtensions
    {
        public static async Task<Dictionary<string, bool?>> GetExternalAuthenticationSchemeStateDictionaryAsync(this ISettingManager settingManager, int tenantId)
        {
            return new Dictionary<string, bool?>
            {
                { "OpenIdConnect", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated, tenantId) },
                { "Microsoft", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated, tenantId) },
                { "Google", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated, tenantId) },
                { "Twitter", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated, tenantId) },
                { "Facebook", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated, tenantId) },
                { "WsFederation", !await settingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated, tenantId) },
            };
        }
    }
}
