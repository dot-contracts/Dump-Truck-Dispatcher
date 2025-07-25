namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverClockInInput
    {
        public int? TimeClassificationId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string Description { get; set; }
    }
}
