using DispatcherWeb.Caching;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineVehicleCategoryCacheItem : AuditableCacheItem
    {
        public int OrderLineId { get; set; }
        public int VehicleCategoryId { get; set; }
    }
}
