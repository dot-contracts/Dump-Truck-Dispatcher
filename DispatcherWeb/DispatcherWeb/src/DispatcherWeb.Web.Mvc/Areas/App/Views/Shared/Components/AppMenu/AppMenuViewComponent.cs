using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Navigation;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using DispatcherWeb.Web.Areas.App.Models.Layout;
using DispatcherWeb.Web.Areas.App.Startup;
using DispatcherWeb.Web.Views;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.AppMenu
{
    public class AppMenuViewComponent : DispatcherWebViewComponent
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IAbpSession _abpSession;
        private readonly ITenantCache _tenantCache;

        public AppMenuViewComponent(
            IUserNavigationManager userNavigationManager,
            IAbpSession abpSession,
            ITenantCache tenantCache)
        {
            _userNavigationManager = userNavigationManager;
            _abpSession = abpSession;
            _tenantCache = tenantCache;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isLeftMenuUsed, string currentPageName = null)
        {
            var model = new MenuViewModel
            {
                Menu = await _userNavigationManager.GetMenuAsync(AppNavigationProvider.MenuName, await _abpSession.ToUserIdentifierAsync()),
                CurrentPageName = currentPageName,
            };

            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (tenantId == null)
            {
                return GetView(model, isLeftMenuUsed);
            }

            //this was moved to AppNavigationProvider for consistency
            //if (!await SettingManager.DispatchViaAny())
            //{
            //    HideMenuItem(model, AppPageNames.Tenant.Dispatches);
            //    HideMenuItem(model, AppPageNames.Tenant.TruckDispatchList);
            //}

            var tenant = await _tenantCache.GetAsync(tenantId.Value);
            if (tenant.EditionId.HasValue)
            {
                return GetView(model, isLeftMenuUsed);
            }

            var subscriptionManagement = FindMenuItemOrNull(model.Menu.Items, AppPageNames.Tenant.SubscriptionManagement);
            if (subscriptionManagement != null)
            {
                subscriptionManagement.IsVisible = false;
            }

            return GetView(model, isLeftMenuUsed);
        }

        private void HideMenuItem(MenuViewModel model, string menuItemName)
        {
            var menuItem = FindMenuItemOrNull(model.Menu.Items, menuItemName);
            if (menuItem != null)
            {
                menuItem.IsVisible = false;
            }
        }

        public UserMenuItem FindMenuItemOrNull(IList<UserMenuItem> userMenuItems, string name)
        {
            if (userMenuItems == null)
            {
                return null;
            }

            foreach (var menuItem in userMenuItems)
            {
                if (menuItem.Name == name)
                {
                    return menuItem;
                }

                var found = FindMenuItemOrNull(menuItem.Items, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private IViewComponentResult GetView(MenuViewModel model, bool isLeftMenuUsed)
        {
            return View(isLeftMenuUsed ? "Default" : "Top", model);
        }
    }
}
