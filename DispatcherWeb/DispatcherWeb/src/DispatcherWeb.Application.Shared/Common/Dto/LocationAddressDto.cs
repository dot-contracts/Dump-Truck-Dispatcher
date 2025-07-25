namespace DispatcherWeb.Common.Dto
{
    public class LocationAddressDto
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }

        public string FormattedAddress => Utilities.FormatAddress(StreetAddress, City, State, ZipCode, CountryCode);
    }
}
