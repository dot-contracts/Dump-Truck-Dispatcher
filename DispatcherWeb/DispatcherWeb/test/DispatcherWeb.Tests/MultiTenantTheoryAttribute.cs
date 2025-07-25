using Xunit;

namespace DispatcherWeb.Tests
{
    public sealed class MultiTenantTheoryAttribute : TheoryAttribute
    {
        public MultiTenantTheoryAttribute()
        {
            //we don't need to skip the multitenancy tests
            //if (!appConfiguration.IsMultitenancyEnabled())
            //{
            //    Skip = "MultiTenancy is disabled.";
            //}
        }
    }
}