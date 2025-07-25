namespace DispatcherWeb.Customers.Dto
{
    public class CustomerSelectListInfoDto
    {
        public string AccountNumber { get; set; }

        public bool CustomerIsCod { get; set; }

        public int? PricingTierId { get; set; }

        public bool IsTaxExempt { get; set; }
    }
}
