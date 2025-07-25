using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IUserRoleEntityListCache : IListCache<ListCacheTenantKey, UserRoleCacheItem, long>
    {
    }
}
