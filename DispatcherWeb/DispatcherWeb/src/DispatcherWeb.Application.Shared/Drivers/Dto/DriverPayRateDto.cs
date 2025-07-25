namespace DispatcherWeb.Drivers.Dto
{
    public class DriverPayRateDto
    {
        public int? TimeClassificationId { get; set; }
        public decimal? PayRate { get; set; }
        public bool? IsProductionBased { get; set; }
        public string DriverName { get; set; }
        public bool DriverIsInactive { get; set; }
    }
}
