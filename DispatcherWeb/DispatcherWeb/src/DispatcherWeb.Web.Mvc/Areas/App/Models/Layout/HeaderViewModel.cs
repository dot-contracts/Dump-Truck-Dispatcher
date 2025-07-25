using System.Collections.Generic;
using Abp.Localization;
using DispatcherWeb.Sessions.Dto;
using DispatcherWeb.Web.Utils;

namespace DispatcherWeb.Web.Areas.App.Models.Layout
{
    public class HeaderViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

        public IReadOnlyList<LanguageInfo> Languages { get; set; }

        public LanguageInfo CurrentLanguage { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public bool IsImpersonatedLogin { get; set; }

        public int SubscriptionExpireNootifyDayCount { get; set; }

        public string GetShownLoginName()
        {
            var userName = "<span id=\"HeaderCurrentUserName\">" + HtmlHelper.Sanitize(LoginInformations.User.UserName) + "</span>";

            if (!IsMultiTenancyEnabled)
            {
                return userName;
            }

            return LoginInformations.Tenant == null
                ? "<span class='tenancy-name'>.\\</span>" + userName
                : "<span class='tenancy-name'>" + HtmlHelper.Sanitize(LoginInformations.Tenant.TenancyName) + "\\" + "</span>" + userName;
        }

        public string GetLogoUrl(string appPath)
        {
            if (LoginInformations?.Tenant?.LogoId == null)
            {
                return appPath + "Common/Images/app-logo-dump-truck-130x35.gif";
            }

            //id parameter is used to prevent caching only.
            return appPath + "TenantCustomization/GetLogo?id=" + LoginInformations.Tenant.LogoId;
        }

        public string GetProfilePictureUrl(string appPath)
        {
            if (LoginInformations.User.ProfilePictureId == null)
            {
                return appPath + "Common/Images/default-profile-picture.png";
            }
            return appPath + "Profile/GetProfilePictureById?id=" + LoginInformations.User.ProfilePictureId;
        }
    }
}
