using DispatcherWeb.Caching;

namespace DispatcherWeb.Trucks.Dto
{
    public class TruckListCacheItem : AuditableCacheItem
    {
        public string TruckCode { get; set; }
        public int? OfficeId { get; set; }
        public int VehicleCategoryId { get; set; }
        public bool CanPullTrailer { get; set; }
        public bool IsOutOfService { get; set; }
        public bool IsActive { get; set; }
        public int? DefaultDriverId { get; set; }
        public BedConstructionEnum? BedConstruction { get; set; }
        public int? Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool IsApportioned { get; set; }
        public int? CurrentTrailerId { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
        public decimal? CargoCapacity { get; set; }
        public decimal? CargoCapacityCyds { get; set; }
    }
}
