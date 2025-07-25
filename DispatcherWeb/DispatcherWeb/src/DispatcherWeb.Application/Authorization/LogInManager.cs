using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;
using Microsoft.AspNetCore.Identity;

namespace DispatcherWeb.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        private readonly IAbpStartupConfiguration _startupConfiguration;

        public LogInManager(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            UserClaimsPrincipalFactory claimsPrincipalFactory,
            IAbpStartupConfiguration startupConfiguration)
            : base(
                  userManager,
                  multiTenancyConfig,
                  tenantRepository,
                  unitOfWorkManager,
                  settingManager,
                  userLoginAttemptRepository,
                  userManagementConfig,
                  iocResolver,
                  passwordHasher,
                  roleManager,
                  claimsPrincipalFactory)
        {
            _startupConfiguration = startupConfiguration;

        }

        [UnitOfWork]
        public virtual async Task<AbpLoginResult<Tenant, User>> ForceHostLoginAsync(string userNameOrEmailAddress, string plainPassword, bool shouldLockout = true)
        {
            var result = await ForceHostLoginAsyncInternal(userNameOrEmailAddress, plainPassword, shouldLockout);
            if (result.Result == AbpLoginResultType.Success)
            {
                _startupConfiguration.MultiTenancy.IsEnabled = true;
            }
            await SaveLoginAttemptAsync(result, null, userNameOrEmailAddress);
            return result;
        }

        protected async Task<AbpLoginResult<Tenant, User>> ForceHostLoginAsyncInternal(string userNameOrEmailAddress, string plainPassword, bool shouldLockout)
        {
            if (userNameOrEmailAddress.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(userNameOrEmailAddress));
            }

            if (plainPassword.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(plainPassword));
            }

            var tenantId = (int?)null;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                await UserManager.InitializeOptionsAsync(tenantId);

                //TryLoginFromExternalAuthenticationSources method may create the user, that's why we are calling it before AbpUserStore.FindByNameOrEmailAsync
                var loggedInFromExternalSource = await TryLoginFromExternalAuthenticationSourcesAsync(userNameOrEmailAddress, plainPassword, null);

                var user = await UserManager.FindByNameOrEmailAsync(tenantId, userNameOrEmailAddress);
                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, null, user);
                }

                if (!loggedInFromExternalSource)
                {
                    if (!await UserManager.CheckPasswordAsync(user, plainPassword))
                    {
                        if (shouldLockout)
                        {
                            if (await TryLockOutAsync(tenantId, user.Id))
                            {
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, null, user);
                            }
                        }

                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, null, user);
                    }

                    await UserManager.ResetAccessFailedCountAsync(user);
                }

                return await CreateLoginResultAsync(user, null);
            }
        }
    }
}
