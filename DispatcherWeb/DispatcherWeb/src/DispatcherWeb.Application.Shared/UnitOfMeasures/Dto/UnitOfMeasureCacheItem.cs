using DispatcherWeb.Caching;

namespace DispatcherWeb.UnitOfMeasures.Dto
{
    public class UnitOfMeasureCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public UnitOfMeasureBaseEnum? UomBaseId { get; set; }
    }
}
