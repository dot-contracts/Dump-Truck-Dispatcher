namespace DispatcherWeb.Distance.Dto
{
    public class LocationDto : ILocation
    {
        public int Id { get; set; }

        public string PlaceId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
    }
}
