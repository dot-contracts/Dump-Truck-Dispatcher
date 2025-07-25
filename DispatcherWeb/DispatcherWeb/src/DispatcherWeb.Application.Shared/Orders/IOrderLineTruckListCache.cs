using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders
{
    public interface IOrderLineTruckListCache : IListCache<ListCacheDateKey, OrderLineTruckCacheItem>
    {
    }
}
