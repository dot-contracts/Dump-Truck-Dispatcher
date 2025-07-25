using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders
{
    public interface IOrderLineListCache : IListCache<ListCacheDateKey, OrderLineCacheItem>
    {
    }
}
