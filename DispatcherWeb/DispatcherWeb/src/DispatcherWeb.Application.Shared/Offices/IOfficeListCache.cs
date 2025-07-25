using DispatcherWeb.Caching;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Offices
{
    public interface IOfficeListCache : IListCache<ListCacheTenantKey, OfficeCacheItem>
    {
    }
}
