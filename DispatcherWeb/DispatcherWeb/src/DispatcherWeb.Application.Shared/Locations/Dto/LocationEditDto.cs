using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Locations.Dto
{
    public class LocationEditDto
    {
        public int? Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string DisplayName => Utilities.FormatAddress(Name, StreetAddress, City, State, null);

        [Required]
        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public bool IsTemporary { get; set; }

        public bool MergeWithDuplicateSilently { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string PlaceId { get; set; }

        public bool IsActive { get; set; }

        public bool SendToFulcrum { get; set; }

        [StringLength(10)]
        public string Abbreviation { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
