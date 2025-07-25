using DispatcherWeb.Caching;

namespace DispatcherWeb.Locations.Dto
{
    public class LocationCacheItem : AuditableCacheItem
    {
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}
