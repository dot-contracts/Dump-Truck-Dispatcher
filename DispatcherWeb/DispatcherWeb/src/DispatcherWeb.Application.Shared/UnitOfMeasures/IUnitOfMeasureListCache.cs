using DispatcherWeb.Caching;
using DispatcherWeb.UnitOfMeasures.Dto;

namespace DispatcherWeb.UnitOfMeasures
{
    public interface IUnitOfMeasureListCache : IListCache<ListCacheTenantKey, UnitOfMeasureCacheItem>
    {
    }
}
