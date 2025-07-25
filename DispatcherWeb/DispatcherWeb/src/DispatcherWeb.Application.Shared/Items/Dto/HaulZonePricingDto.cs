namespace DispatcherWeb.Items.Dto
{
    public class HaulZonePricingDto
    {
        public decimal? PricePerUnit { get; set; }

        public decimal LeaseHaulerRate { get; set; }

        public bool IsMultiplePriceObject { get; set; }
    }
}
