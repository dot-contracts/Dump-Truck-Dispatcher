namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class PaymentSettingsEditDto
    {
        public PaymentProcessor PaymentProcessor { get; set; }
        public string HeartlandPublicKey { get; set; }
        public string HeartlandSecretKey { get; set; }
    }
}
