using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IUserLoginEntityListCache : IListCache<ListCacheTenantKey, UserLoginCacheItem, long>
    {
    }
}
