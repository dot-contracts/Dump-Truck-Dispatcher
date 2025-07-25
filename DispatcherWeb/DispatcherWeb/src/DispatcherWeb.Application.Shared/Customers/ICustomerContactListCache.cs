using DispatcherWeb.Caching;
using DispatcherWeb.Customers.Dto;

namespace DispatcherWeb.Customers
{
    public interface ICustomerContactListCache : IListCache<ListCacheTenantKey, CustomerContactCacheItem>
    {
    }
}
