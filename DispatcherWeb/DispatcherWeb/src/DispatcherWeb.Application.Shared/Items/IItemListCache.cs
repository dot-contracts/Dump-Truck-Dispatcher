using DispatcherWeb.Caching;
using DispatcherWeb.Items.Dto;

namespace DispatcherWeb.Items
{
    public interface IItemListCache : IListCache<ListCacheTenantKey, ItemCacheItem>
    {
    }
}
