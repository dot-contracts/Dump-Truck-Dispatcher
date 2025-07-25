using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface IAvailableLeaseHaulerTruckListCache : IListCache<ListCacheDateKey, AvailableLeaseHaulerTruckCacheItem>
    {
    }
}
