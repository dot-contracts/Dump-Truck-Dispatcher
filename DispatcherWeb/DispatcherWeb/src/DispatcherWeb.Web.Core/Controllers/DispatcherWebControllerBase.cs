using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.IdentityFramework;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DispatcherWeb.Web.Controllers
{
    public abstract class DispatcherWebControllerBase : AbpController
    {
        protected DispatcherWebControllerBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected void SetTenantIdCookie(int? tenantId)
        {
            var multiTenancyConfig = HttpContext.RequestServices.GetRequiredService<IMultiTenancyConfig>();
            var configurationAccessor = HttpContext.RequestServices.GetRequiredService<IAppConfigurationAccessor>();
            Response.Cookies.Append(
                multiTenancyConfig.TenantIdResolveKey,
                tenantId?.ToString() ?? string.Empty,
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddYears(5),
                    Path = "/",
                    Domain = configurationAccessor.Configuration.GetCookieDomain(),
                    SameSite = SameSiteMode.None,
                    Secure = true,
                }
            );
        }
        protected async Task<DateTime> GetToday()
        {
            try
            {
                var timeZone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
                return TimeExtensions.GetToday(timeZone);
            }
            catch (Exception)
            {
                // Return UTC time if there's an error
                return TimeExtensions.GetToday("UTC");
            }
        }

        protected FileContentResult InlinePdfFile(byte[] fileContents, string fileName)
        {
            string mimeType = "application/pdf";
            Response.Headers["Content-Disposition"] = "inline; filename=" + fileName.SanitizeFilename();
            return File(fileContents, mimeType);
        }
    }
}
