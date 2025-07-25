using DispatcherWeb.Editions;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Security;

namespace DispatcherWeb.Web.Models.TenantRegistration
{
    public class TenantRegisterViewModel
    {
        public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

        public int? EditionId { get; set; }

        public string PaymentId { get; set; }

        public SubscriptionPaymentGatewayType? Gateway { get; set; }

        public SubscriptionStartType? SubscriptionStartType { get; set; }

        public EditionSelectDto Edition { get; set; }

        public EditionPaymentType EditionPaymentType { get; set; }

        public bool ShowPaymentExpireNotification()
        {
            return !string.IsNullOrEmpty(PaymentId);
        }
    }
}
