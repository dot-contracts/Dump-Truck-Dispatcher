using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Runtime.Session;
using DispatcherWeb.Authentication.TwoFactor;
using DispatcherWeb.Authorization.Delegation;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public class SessionInfoCache : ISessionInfoCache, ITransientDependency
    {
        private readonly IUserSessionInfoCache _userSessionInfoCache;
        private readonly ITenantSessionInfoCache _tenantSessionInfoCache;
        private readonly IUserDelegationConfiguration _userDelegationConfiguration;

        public SessionInfoCache(
            IAbpSession session,
            IUserSessionInfoCache userSessionInfoCache,
            ITenantSessionInfoCache tenantSessionInfoCache,
            IUserDelegationConfiguration userDelegationConfiguration
        )
        {
            Session = session;
            _userSessionInfoCache = userSessionInfoCache;
            _tenantSessionInfoCache = tenantSessionInfoCache;
            _userDelegationConfiguration = userDelegationConfiguration;
        }

        public IAbpSession Session { get; }

        public async Task<GetCurrentLoginInformationsOutput> GetSessionInfoFromCacheOrSource()
        {
            var output = new GetCurrentLoginInformationsOutput
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>(),
                    Currency = DispatcherWebConsts.Currency,
                    CurrencySign = DispatcherWebConsts.CurrencySign,
                    AllowTenantsToChangeEmailSettings = DispatcherWebConsts.AllowTenantsToChangeEmailSettings,
                    UserDelegationIsEnabled = _userDelegationConfiguration.IsEnabled,
                    TwoFactorCodeExpireSeconds = TwoFactorCodeCacheItem.DefaultSlidingExpireTime.TotalSeconds,
                },
            };

            var tenantId = await Session.GetTenantIdOrNullAsync();
            if (tenantId.HasValue)
            {
                output.Tenant = await _tenantSessionInfoCache.GetTenantSessionInfoFromCacheOrSource(tenantId.Value);
            }

            if (Session.ImpersonatorTenantId.HasValue)
            {
                output.ImpersonatorTenant = await _tenantSessionInfoCache.GetTenantSessionInfoFromCacheOrSource(Session.ImpersonatorTenantId.Value);
            }

            if (Session.UserId.HasValue)
            {
                output.User = await _userSessionInfoCache.GetUserSessionInfoFromCacheOrSource(Session.UserId.Value);
            }

            if (Session.ImpersonatorUserId.HasValue)
            {
                output.ImpersonatorUser = await _userSessionInfoCache.GetUserSessionInfoFromCacheOrSource(Session.ImpersonatorUserId.Value, true);
            }

            return output;
        }

        public async Task InvalidateCache()
        {
            await _userSessionInfoCache.InvalidateCache();
            await _tenantSessionInfoCache.InvalidateCache();
        }

        public async Task InvalidateCacheForUser(long userId)
        {
            await _userSessionInfoCache.InvalidateCache(userId);
        }

        public async Task InvalidateCacheForTenant(int tenantId)
        {
            await _tenantSessionInfoCache.InvalidateCache(tenantId);
        }
    }
}
