namespace DispatcherWeb.Items.Dto
{
    public class ItemPricingDto
    {
        public decimal? PricePerUnit { get; set; }

        public decimal? MaterialCostRate { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public bool HasPricing { get; set; }

        public bool? IsMultiplePriceObject { get; set; }

        public QuoteLinePricingDto QuoteBasedPricing { get; set; }

        public decimal? LeaseHaulerRate { get; set; }
    }
}
