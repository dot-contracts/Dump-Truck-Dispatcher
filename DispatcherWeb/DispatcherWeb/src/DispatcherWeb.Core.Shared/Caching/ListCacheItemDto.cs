using System;

namespace DispatcherWeb.Caching
{
    public class ListCacheItemDto<TItem>
        : ListCacheItemDto<ListCacheTenantKey, TItem, int>
        where TItem : IAuditableCacheItem, new()
    {
    }

    public class ListCacheItemDto<TListKey, TItem>
        : ListCacheItemDto<TListKey, TItem, int>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem, new()
    {
    }

    public class ListCacheItemDto<TListKey, TItem, TItemKey>
        : ListCacheItem<TListKey, TItem, TItemKey>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<TItemKey>
        where TItemKey : struct, IEquatable<TItemKey>
    {
        /// <summary>
        /// Contains Max(CreationTime, ModificationTime, DeletionTime) across all entries in the current backend cache, can be sent back as a AfterDateTime parameter
        /// </summary>
        public DateTime? MaxDateTime { get; set; }

        /// <summary>
        /// Can be set to true to indicate to the client that their cache has to be hard invalidated and the returned changes are not incremental
        /// </summary>
        public bool HardInvalidate { get; set; }
    }
}
