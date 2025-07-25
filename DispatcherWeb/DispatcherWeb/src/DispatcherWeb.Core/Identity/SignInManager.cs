using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using DispatcherWeb.Authentication;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DispatcherWeb.Identity
{
    public class SignInManager : AbpSignInManager<Tenant, Role, User>
    {
        private readonly ISettingManager _settingManager;
        private readonly IAbpSession _abpSession;

        public SignInManager(
            UserManager userManager,
            IHttpContextAccessor contextAccessor,
            UserClaimsPrincipalFactory claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<User>> logger,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<User> userConfirmation,
            IAbpSession abpSession)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, unitOfWorkManager, settingManager, schemes, userConfirmation)
        {
            _settingManager = settingManager;
            _abpSession = abpSession;
        }

        public override async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            var schemes = await base.GetExternalAuthenticationSchemesAsync();

            var tenantId = await _abpSession.GetTenantIdOrNullAsync();
            if (tenantId.HasValue)
            {
                var settings = await _settingManager.GetExternalAuthenticationSchemeStateDictionaryAsync(tenantId.Value);

                schemes = schemes.Where(x => settings.GetValueOrDefault(x.Name) ?? true);
            }

            return schemes;
        }
    }

}
