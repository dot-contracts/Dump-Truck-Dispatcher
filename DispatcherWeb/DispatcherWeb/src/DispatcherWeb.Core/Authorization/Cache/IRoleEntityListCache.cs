using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IRoleEntityListCache : IListCache<ListCacheTenantKey, Role>
    {
    }
}
