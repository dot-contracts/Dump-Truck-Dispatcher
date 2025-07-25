namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TruxSettingsEditDto
    {
        public bool AllowImportingTruxEarnings { get; set; }
        public int? TruxCustomerId { get; set; }
        public string TruxCustomerName { get; set; }
        public bool UseForProductionPay { get; set; }
    }
}
