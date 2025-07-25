using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerListCache : IListCache<ListCacheTenantKey, LeaseHaulerCacheItem>
    {
    }
}
