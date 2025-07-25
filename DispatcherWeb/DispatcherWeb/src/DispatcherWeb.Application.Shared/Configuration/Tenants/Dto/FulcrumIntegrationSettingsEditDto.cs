namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class FulcrumIntegrationSettingsEditDto
    {
        public bool FulcrumIntegrationIsEnabled { get; set; }
        public string FulcrumCustomerNumber { get; set; }
        public string FulcrumUserName { get; set; }
        public string FulcrumPassword { get; set; }
    }
}
