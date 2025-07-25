using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.MultiTenancy;
using DispatcherWeb.Url;

namespace DispatcherWeb.Web.Url
{
    public abstract class AppUrlServiceBase : IAppUrlService, ITransientDependency
    {
        public abstract string EmailActivationRoute { get; }

        public abstract string PasswordResetRoute { get; }

        protected readonly IWebUrlService WebUrlService;

        protected readonly ITenantCache TenantCache;

        protected AppUrlServiceBase(IWebUrlService webUrlService, ITenantCache tenantCache)
        {
            WebUrlService = webUrlService;
            TenantCache = tenantCache;
        }

        public async Task<string> CreateEmailActivationUrlFormatAsync(int? tenantId)
        {
            return CreateEmailActivationUrlFormat(await GetTenancyNameAsync(tenantId));
        }

        public async Task<string> CreatePasswordResetUrlFormatAsync(int? tenantId)
        {
            return CreatePasswordResetUrlFormat(await GetTenancyNameAsync(tenantId));
        }

        public string CreateEmailActivationUrlFormat(string tenancyName)
        {
            var activationLink = WebUrlService.GetSiteRootAddress(tenancyName).EnsureEndsWith('/') + EmailActivationRoute +
                "?userId={userId}&confirmationCode={confirmationCode}";

            if (tenancyName != null)
            {
                activationLink += "&tenantId={tenantId}";
            }

            return activationLink;
        }

        public string CreatePasswordResetUrlFormat(string tenancyName)
        {
            var resetLink = WebUrlService.GetSiteRootAddress(tenancyName).EnsureEndsWith('/') + PasswordResetRoute +
                "?userId={userId}&resetCode={resetCode}";

            if (tenancyName != null)
            {
                resetLink += "&tenantId={tenantId}";
            }

            return resetLink;
        }

        public string CreateLeaseHaulerInvitationUrlFormat(Guid oneTimeLoginId)
        {
            var invitationLink = WebUrlService.GetSiteRootAddress().EnsureEndsWith('/') + "Account/LoginByLink/" + oneTimeLoginId;

            return invitationLink;
        }

        public string CreateLinkToSchedule(int? tenantId)
        {
            return WebUrlService.GetSiteRootAddress().EnsureEndsWith('/') + "Account/SchedulingForTenant/" + tenantId;
        }

        private async Task<string> GetTenancyNameAsync(int? tenantId)
        {
            if (!tenantId.HasValue)
            {
                return null;
            }

            var tenant = await TenantCache.GetAsync(tenantId.Value);
            return tenant.TenancyName;
        }
    }
}
