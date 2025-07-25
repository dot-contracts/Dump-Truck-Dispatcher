using DispatcherWeb.Caching;

namespace DispatcherWeb.VehicleCategories.Dto
{
    public class VehicleCategoryCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public AssetType AssetType { get; set; }
        public bool IsPowered { get; set; }
        public int SortOrder { get; set; }
    }
}
