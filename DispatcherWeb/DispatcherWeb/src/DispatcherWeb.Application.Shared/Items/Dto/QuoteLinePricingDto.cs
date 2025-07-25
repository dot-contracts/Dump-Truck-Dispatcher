namespace DispatcherWeb.Items.Dto
{
    public class QuoteLinePricingDto
    {
        public decimal? PricePerUnit { get; set; }

        public decimal? MaterialCostRate { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public decimal? LeaseHaulerRate { get; set; }
    }
}
