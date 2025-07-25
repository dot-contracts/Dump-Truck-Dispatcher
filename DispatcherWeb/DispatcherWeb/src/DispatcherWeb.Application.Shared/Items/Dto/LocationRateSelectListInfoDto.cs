namespace DispatcherWeb.Items.Dto
{
    public class LocationRateSelectListInfoDto
    {
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal? MaterialCostRate { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public decimal? Distance { get; set; }
        public decimal? CombinedRate { get; set; }
    }
}
