using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerDriverListCache : IListCache<ListCacheTenantKey, LeaseHaulerDriverCacheItem>
    {
    }
}
