namespace DispatcherWeb.Caching
{
    public class ListCacheTenantKey : ListCacheKey
    {
        public ListCacheTenantKey()
        {
        }

        public ListCacheTenantKey(int tenantId)
        {
            TenantId = tenantId;
        }

        public int TenantId { get; set; }

        public override string ToStringKey()
        {
            return $"{TenantId}";
        }
    }
}
