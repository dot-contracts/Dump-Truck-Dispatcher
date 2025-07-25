using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerTruckListCache : IListCache<ListCacheTenantKey, LeaseHaulerTruckCacheItem>
    {
    }
}
