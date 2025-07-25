using DispatcherWeb.Common.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.DriverApp.Locations.Dto
{
    public class LocationDto
    {
        public string Name { get; set; }

        [JsonIgnore]
        public LocationAddressDto AddressObject { get; set; }

        public string Address => AddressObject?.FormattedAddress;


        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
