using DispatcherWeb.Caching;
using DispatcherWeb.Drivers.Dto;

namespace DispatcherWeb.Drivers
{
    public interface IDriverListCache : IListCache<ListCacheTenantKey, DriverCacheItem>
    {
    }
}
