namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class IronSheepdogSettingsEditDto
    {
        public bool AllowImportingIronSheepdogEarnings { get; set; }
        public int? IronSheepdogCustomerId { get; set; }
        public string IronSheepdogCustomerName { get; set; }
        public bool UseForProductionPay { get; set; }
    }
}
