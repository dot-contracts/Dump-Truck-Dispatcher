using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IUserOrganizationUnitEntityListCache : IListCache<ListCacheTenantKey, UserOrganizationUnitCacheItem, long>
    {
    }
}
