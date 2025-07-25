using DispatcherWeb.MultiTenancy.Payments;

namespace DispatcherWeb.Web.Models.Payment
{
    public class CancelPaymentModel
    {
        public string PaymentId { get; set; }

        public SubscriptionPaymentGatewayType Gateway { get; set; }
    }
}