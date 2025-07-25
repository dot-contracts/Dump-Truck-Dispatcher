using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders
{
    public interface IOrderLineVehicleCategoryListCache : IListCache<ListCacheDateKey, OrderLineVehicleCategoryCacheItem>
    {
    }
}
