using System;
using System.Threading.Tasks;
using DispatcherWeb.Caching.Dto;

namespace DispatcherWeb.Caching
{
    public interface IListCache<TListKey, TItem>
        : IListCache<TListKey, TItem, int>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<int>, new()
    {
    }

    public interface IListCache<TListKey, TItem, TItemKey>
        : IListCache<TListKey>
        where TListKey : ListCacheKey
        where TItem : IAuditableCacheItem<TItemKey>
        where TItemKey : struct, IEquatable<TItemKey>
    {
        Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetListOrThrow(TListKey key);
        Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetList(GetListCacheListInput<TListKey> input);
        Task<ListCacheItemDto<TListKey, TItem, TItemKey>> GetList(TListKey key);
    }

    // ReSharper disable once TypeParameterCanBeVariant - it cannot be, it allows incorrect casts in this case
    public interface IListCache<TListKey>
        : IListCache
    {
        Task MarkAsStale(TListKey key);
        Task InvalidateCache(TListKey key);
        Task HardInvalidateCache(TListKey key);
    }

    public interface IListCache
    {
        string CacheName { get; }
        Task<bool> IsEnabled();
        Task<bool> IsFrontendCacheEnabled();

        /// <summary>
        /// Mark key as stale without propagating the cache invalidation instruction to other nodes or clients.
        /// </summary>
        Task MarkAsStaleInternal(string key);
    }
}
