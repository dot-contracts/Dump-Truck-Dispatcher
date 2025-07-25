using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IOrganizationUnitRoleEntityListCache : IListCache<ListCacheTenantKey, OrganizationUnitRoleCacheItem, long>
    {
    }
}
