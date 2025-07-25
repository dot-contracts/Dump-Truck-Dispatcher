namespace DispatcherWeb.DriverApp.TimeClassifications.Dto
{
    public class TimeClassificationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsProductionBased { get; set; }
    }
}
