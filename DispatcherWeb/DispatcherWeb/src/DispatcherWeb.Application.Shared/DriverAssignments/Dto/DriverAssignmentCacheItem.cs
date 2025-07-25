using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class DriverAssignmentCacheItem : AuditableCacheItem
    {
        public Shift? Shift { get; set; }
        public DateTime? StartTime { get; set; }
        public int? OfficeId { get; set; }
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
    }
}
