namespace DispatcherWeb.Caching
{
    public class ListCacheSyncRequest<TListKey>
        where TListKey : ListCacheKey
    {
        public string CacheName { get; set; }
        public TListKey Key { get; set; }
        public int? TenantId { get; set; }
    }
}
