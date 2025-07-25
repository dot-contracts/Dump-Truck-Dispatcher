using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Users
{
    public interface IUserListCache : IListCache<ListCacheTenantKey, UserCacheItem, long>
    {
    }
}
