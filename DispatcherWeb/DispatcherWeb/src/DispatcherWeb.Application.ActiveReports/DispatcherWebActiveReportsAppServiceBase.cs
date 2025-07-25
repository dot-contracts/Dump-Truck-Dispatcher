using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Exceptions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.ActiveReports
{
    /// <summary>
    /// Derive your driver app application services from this class.
    /// </summary>
    public abstract class DispatcherWebActiveReportsAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }
        public IExtendedAbpSession Session { get; set; }

        protected DispatcherWebActiveReportsAppServiceBase()
        {
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

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected int OfficeId
        {
            get
            {
                if (Session.OfficeId.HasValue)
                {
                    return Session.OfficeId.Value;
                }
                throw new UserFriendlyException("You must have an assigned Office in User Details to use that function");
            }
        }

        protected async Task SaveOrThrowConcurrencyErrorAsync()
        {
            try
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
        }
    }
}
