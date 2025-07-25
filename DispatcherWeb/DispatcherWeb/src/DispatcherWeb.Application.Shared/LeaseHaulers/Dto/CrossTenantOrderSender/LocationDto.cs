using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class LocationDto
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Abbreviation { get; set; }
        public LocationCategoryDto Category { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string CountryCode { get; set; }
        public string Notes { get; set; }
        public string PlaceId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<LocationContactDto> LocationContacts { get; set; }
    }
}
