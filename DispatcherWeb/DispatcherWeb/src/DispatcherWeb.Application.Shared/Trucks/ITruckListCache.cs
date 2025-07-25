using DispatcherWeb.Caching;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckListCache : IListCache<ListCacheTenantKey, TruckListCacheItem>
    {
    }
}
