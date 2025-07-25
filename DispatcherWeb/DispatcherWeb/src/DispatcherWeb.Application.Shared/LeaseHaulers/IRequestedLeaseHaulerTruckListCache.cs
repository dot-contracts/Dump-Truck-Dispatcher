using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface IRequestedLeaseHaulerTruckListCache : IListCache<ListCacheDateKey, RequestedLeaseHaulerTruckCacheItem>
    {
    }
}
