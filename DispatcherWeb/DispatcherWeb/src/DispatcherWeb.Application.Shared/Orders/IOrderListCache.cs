using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders
{
    public interface IOrderListCache : IListCache<ListCacheDateKey, OrderCacheItem>
    {
    }
}
