using Abp.Application.Navigation;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.AppMenu
{
    public static class UserMenuItemExtensions
    {
        public static bool IsMenuActive(this UserMenuItem menuItem, string currentPageName)
        {
            if (menuItem.Name == currentPageName)
            {
                return true;
            }

            if (menuItem.Items != null)
            {
                foreach (var subMenuItem in menuItem.Items)
                {
                    if (subMenuItem.IsMenuActive(currentPageName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string CalculateUrl(this UserMenuItem menuItem, string applicationPath)
        {
            if (string.IsNullOrEmpty(menuItem.Url))
            {
                return applicationPath;
            }

            if (IsRooted(menuItem.Url))
            {
                return menuItem.Url;
            }

            return applicationPath + menuItem.Url;
        }

        private static bool IsRooted(string url)
        {
            return url != null && url.StartsWith("/");
        }
    }
}
