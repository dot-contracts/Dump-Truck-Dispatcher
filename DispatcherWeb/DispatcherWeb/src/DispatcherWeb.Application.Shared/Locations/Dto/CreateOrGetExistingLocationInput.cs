using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Locations.Dto
{
    public class CreateOrGetExistingLocationInput
    {
        public string Name { get; set; }

        [Required]
        public PredefinedLocationCategoryKind? PredefinedLocationCategoryKind { get; set; }

        public string CategoryName { get; set; }

        public bool IsTemporary { get; set; }

        public bool MergeWithDuplicateSilently { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string PlaceId { get; set; }

        public bool IsActive { get; set; }

        public bool SendToFulcrum { get; set; }

        public string Abbreviation { get; set; }

        public string Notes { get; set; }
    }
}
