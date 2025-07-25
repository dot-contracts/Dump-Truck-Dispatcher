namespace DispatcherWeb.VehicleServices.Dto
{
    public class VehicleServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? RecommendedTimeInterval { get; set; }
        public decimal? RecommendedHourInterval { get; set; }
        public int? RecommendedMileageInterval { get; set; }
    }
}
