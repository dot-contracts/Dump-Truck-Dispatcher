using System;
using System.Text;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Runtime.Session;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public class SessionAppService : DispatcherWebAppServiceBase, ISessionAppService
    {
        private readonly ISessionInfoCache _sessionInfoCache;

        public SessionAppService(
            ISessionInfoCache sessionInfoCache
            )
        {
            _sessionInfoCache = sessionInfoCache;
        }

        [DisableAuditing]
        [AbpAllowAnonymous]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            return await _sessionInfoCache.GetSessionInfoFromCacheOrSource();
        }

        [AbpAuthorize]
        public async Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken()
        {
            if (AbpSession.UserId is null or 0)
            {
                throw new Exception(L("ThereIsNoLoggedInUser"));
            }

            var user = await UserManager.GetUserAsync(await AbpSession.ToUserIdentifierAsync());
            user.SetSignInToken();
            await UserManager.UpdateAsync(user);
            return new UpdateUserSignInTokenOutput
            {
                SignInToken = user.SignInToken,
                EncodedUserId = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.ToString())),
                EncodedTenantId = user.TenantId.HasValue
                    ? Convert.ToBase64String(Encoding.UTF8.GetBytes(user.TenantId.Value.ToString()))
                    : "",
            };
        }
    }
}
