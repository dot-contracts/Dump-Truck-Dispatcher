using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Web.Views.Shared.Components.AccountLogo
{
    public class AccountLogoViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; }

        public AccountLogoViewModel(GetCurrentLoginInformationsOutput loginInformations)
        {
            LoginInformations = loginInformations;
        }

        public string GetLogoUrl(string appPath)
        {
            if (LoginInformations.Tenant?.LogoId == null)
            {
                return appPath + "Common/Images/app-logo-dump-truck-130x35.gif";
            }

            return appPath + "TenantCustomization/GetLogo?id=" + LoginInformations.Tenant.LogoId;
        }
    }
}