using Xunit;

namespace DispatcherWeb.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute()
        {
            //we don't need to skip the multitenancy tests
            //if (!appConfiguration.IsMultitenancyEnabled())
            //{
            //    Skip = "MultiTenancy is disabled.";
            //}
        }
    }
}
