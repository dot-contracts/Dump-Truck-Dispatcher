using DispatcherWeb.Caching;

namespace DispatcherWeb.Offices.Dto
{
    public class OfficeCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string TruckColor { get; set; }
        public bool CopyDeliverToLoadAtChargeTo { get; set; }
    }
}
