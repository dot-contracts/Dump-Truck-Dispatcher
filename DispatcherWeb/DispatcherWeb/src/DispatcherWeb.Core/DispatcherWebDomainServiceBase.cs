using System;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Domain.Services;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sessions;

namespace DispatcherWeb
{
    public abstract class DispatcherWebDomainServiceBase : DomainService
    {
        public UserManager UserManager { get; set; }

        public IExtendedAbpSession Session { get; set; }

        public IFeatureChecker FeatureChecker { protected get; set; }
        public IPermissionChecker PermissionChecker { protected get; set; }

        protected DispatcherWebDomainServiceBase()
        {
            PermissionChecker = NullPermissionChecker.Instance;
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected async Task<string> GetTimezone()
        {
            return await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
        }

        protected async Task<DateTime> GetToday()
        {
            var timeZone = await GetTimezone();
            return TimeExtensions.GetToday(timeZone);
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(Session.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("The user is not logged in or wasn't found");
            }

            return user;
        }

        protected int OfficeId => Session.GetOfficeIdOrThrow();
    }
}
