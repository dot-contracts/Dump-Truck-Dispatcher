using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Exceptions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Runtime.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp
{
    /// <summary>
    /// Derive your driver app application services from this class.
    /// </summary>
    public abstract class DispatcherWebDriverAppAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        public IExtendedAbpSession Session { get; set; }

        public ICancellationTokenProvider CancellationTokenProvider { get; set; }

        protected DispatcherWebDriverAppAppServiceBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
            CancellationTokenProvider = NullCancellationTokenProvider.Instance;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected async Task<string> GetCurrentUserFullName()
        {
            return await (await UserManager.GetQueryAsync())
                .Where(x => x.Id == AbpSession.GetUserId())
                .Select(x => x.Name + " " + x.Surname)
                .FirstAsync();
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
