namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerTruckDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public string VehicleCategoryName { get; set; }
        public string DefaultDriverName { get; set; }
        public bool IsActive { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
    }
}
