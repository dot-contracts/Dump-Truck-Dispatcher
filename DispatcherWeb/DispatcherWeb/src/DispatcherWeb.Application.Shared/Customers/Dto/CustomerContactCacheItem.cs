using DispatcherWeb.Caching;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactCacheItem : AuditableCacheItem
    {
        public int CustomerId { get; set; }
        public bool HasCustomerPortalAccess { get; set; }
    }
}
