using System.Reflection;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using static DispatcherWeb.DispatcherWebConsts;

namespace DispatcherWeb.Tests.Security.AnonymousAccess
{
    public class AnonymousControllerAccessTests : AnonymousAccessTestsBase<AbpController, AbpMvcAuthorizeAttribute, AllowAnonymousAttribute>
    {
        protected override string[] GetAssemblyNames()
        {
            return new[]
            {
                AssemblyNames.WebMvc,
            };
        }

        protected override BindingFlags GetMethodBindingFlags()
        {
            return BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        }

        public override void TestImplicitAccessPermissions()
        {
            base.TestImplicitAccessPermissions();
        }
    }
}
