using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Caching
{
    public class ListCacheItem<TItem>
        : ListCacheItem<ListCacheTenantKey, TItem, int>
        where TItem : IAuditableCacheItem, new()
    {
    }

    public class ListCacheItem<TListKey, TItem>
        : ListCacheItem<TListKey, TItem, int>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem, new()
    {
    }

    public class ListCacheItem<TListKey, TItem, TItemKey>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<TItemKey>
        where TItemKey : struct, IEquatable<TItemKey>
    {
        public TListKey Key { get; set; }

        public List<TItem> Items { get; set; }

        public DateTime CacheCreationDateTime { get; set; } = DateTime.UtcNow;

        public TItem Find(TItemKey? id)
        {
            if (id == null)
            {
                return default;
            }

            return Find(id.Value);
        }

        public TItem Find(TItemKey id)
        {
            return Items.FirstOrDefault(x => id.Equals(x.Id));
        }
    }
}
