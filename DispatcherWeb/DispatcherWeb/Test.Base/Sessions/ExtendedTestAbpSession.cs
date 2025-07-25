using System;
using Abp;
using Abp.Configuration.Startup;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using Abp.TestBase.Runtime.Session;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.Test.Base.Sessions
{
    public class ExtendedTestAbpSession : TestAbpSession, IExtendedAbpSession
    {
        public ExtendedTestAbpSession(
            IMultiTenancyConfig multiTenancy,
            IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider,
            ITenantResolver tenantResolver
            )
            : base(multiTenancy, sessionOverrideScopeProvider, tenantResolver)
        {
        }

        public int? OfficeId { get; set; }

        public string? OfficeName { get; set; }

        public bool OfficeCopyChargeTo { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public string? UserName { get; set; }

        public string? UserEmail { get; set; }

        public int? LeaseHaulerId { get; set; }

        public IDisposable Use(UserIdentifier userIdentifier)
        {
            return Use(userIdentifier.TenantId, userIdentifier.UserId);
        }
    }
}
