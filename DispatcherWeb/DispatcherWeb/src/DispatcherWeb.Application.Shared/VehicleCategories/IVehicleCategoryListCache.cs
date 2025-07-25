using DispatcherWeb.Caching;
using DispatcherWeb.VehicleCategories.Dto;

namespace DispatcherWeb.VehicleCategories
{
    public interface IVehicleCategoryListCache : IListCache<ListCacheEmptyKey, VehicleCategoryCacheItem>
    {
    }
}
