using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerUserListCache : IListCache<ListCacheTenantKey, LeaseHaulerUserCacheItem>
    {
    }
}
