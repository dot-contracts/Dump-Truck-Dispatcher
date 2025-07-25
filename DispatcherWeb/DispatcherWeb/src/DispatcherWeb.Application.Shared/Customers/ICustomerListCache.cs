using DispatcherWeb.Caching;
using DispatcherWeb.Customers.Dto;

namespace DispatcherWeb.Customers
{
    public interface ICustomerListCache : IListCache<ListCacheTenantKey, CustomerCacheItem>
    {
    }
}
