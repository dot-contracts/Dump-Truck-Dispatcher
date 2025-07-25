using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IUserEntityListCache : IListCache<ListCacheTenantKey, User, long>
    {
    }
}
