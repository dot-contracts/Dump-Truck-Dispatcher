using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
