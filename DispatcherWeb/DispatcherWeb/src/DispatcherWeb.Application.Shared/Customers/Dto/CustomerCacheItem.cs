using DispatcherWeb.Caching;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public bool IsCod { get; set; }
    }
}
