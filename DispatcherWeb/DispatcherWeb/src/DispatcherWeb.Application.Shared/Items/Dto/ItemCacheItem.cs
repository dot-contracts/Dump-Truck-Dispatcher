using DispatcherWeb.Caching;

namespace DispatcherWeb.Items.Dto
{
    public class ItemCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ItemType? Type { get; set; }
        public bool IsTaxable { get; set; }
        public bool UseZoneBasedRates { get; set; }
    }
}
