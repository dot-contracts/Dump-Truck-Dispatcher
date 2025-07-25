using Abp.Application.Services;
using Abp.Authorization;
using static DispatcherWeb.DispatcherWebConsts;

namespace DispatcherWeb.Tests.Security.AnonymousAccess
{
    public class AnonymousAccessTests : AnonymousAccessTestsBase<ApplicationService, AbpAuthorizeAttribute, AbpAllowAnonymousAttribute>
    {
        protected override string[] GetAssemblyNames()
        {
            return new[]
            {
                AssemblyNames.ApplicationApi,
                AssemblyNames.ActiveReportsApi,
                AssemblyNames.DriverAppApi,
            };
        }

        public override void TestImplicitAccessPermissions()
        {
            base.TestImplicitAccessPermissions();
        }
    }
}
