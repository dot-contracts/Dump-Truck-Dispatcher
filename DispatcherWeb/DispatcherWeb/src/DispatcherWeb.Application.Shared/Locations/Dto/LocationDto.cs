namespace DispatcherWeb.Locations.Dto
{
    public class LocationDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CategoryName { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; }

        public PredefinedLocationKind? PredefinedLocationKind { get; set; }

        public bool DisallowDataMerge { get; set; }

        public string Abbreviation { get; set; }

        public string Notes { get; set; }

        public bool SendToFulcrum { get; set; }

    }
}
