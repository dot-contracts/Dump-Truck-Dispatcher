using DispatcherWeb.Distance.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class LocationDistanceRateDto : ILocationDistance
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PlaceId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        public decimal? Distance { get; set; }

        public MaterialPricingDto MaterialPricing { get; set; }

        public class MaterialPricingDto
        {
            public decimal? PricePerUnit { get; set; }

            public decimal? Cost { get; set; }
        }
    }
}
