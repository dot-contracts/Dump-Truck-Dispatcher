namespace DispatcherWeb.Caching
{
    /// <summary>
    /// Use this as a key for caches where a single list is shared across all tenants
    /// </summary>
    public sealed class ListCacheEmptyKey : ListCacheKey
    {
        public static readonly ListCacheEmptyKey Instance = new ListCacheEmptyKey();

        public override string ToStringKey()
        {
            return "CacheItem";
        }
    }
}
