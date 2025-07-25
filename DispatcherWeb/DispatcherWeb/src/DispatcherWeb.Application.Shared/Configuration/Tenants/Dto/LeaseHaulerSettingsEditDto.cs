namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class LeaseHaulerSettingsEditDto
    {
        public string ThankYouForTrucksTemplate { get; set; }
        public bool ShowLeaseHaulerRateOnQuote { get; set; }
        public bool ShowLeaseHaulerRateOnOrder { get; set; }
        public bool AllowSubcontractorsToDriveCompanyOwnedTrucks { get; set; }
        public bool AllowLeaseHaulerRequestProcess { get; set; }
        public bool NotAllowSchedulingLeaseHaulersWithExpiredInsurance { get; set; }
        public decimal BrokerFee { get; set; }
    }
}
