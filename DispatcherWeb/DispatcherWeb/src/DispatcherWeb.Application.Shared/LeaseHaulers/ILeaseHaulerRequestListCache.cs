using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerRequestListCache : IListCache<ListCacheDateKey, LeaseHaulerRequestCacheItem>
    {
    }
}
