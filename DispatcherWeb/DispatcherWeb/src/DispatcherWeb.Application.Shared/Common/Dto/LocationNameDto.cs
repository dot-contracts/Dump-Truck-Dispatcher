namespace DispatcherWeb.Common.Dto
{
    public class LocationNameDto
    {
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public string FormattedAddress => Utilities.FormatAddress(Name, StreetAddress, City, State, null);
    }
}
