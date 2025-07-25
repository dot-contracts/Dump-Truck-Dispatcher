using DispatcherWeb.Caching;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations
{
    public interface ILocationListCache : IListCache<ListCacheTenantKey, LocationCacheItem>
    {
    }
}
