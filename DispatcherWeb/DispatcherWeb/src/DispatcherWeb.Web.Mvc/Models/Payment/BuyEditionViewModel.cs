using System.Collections.Generic;
using DispatcherWeb.Editions;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Payments;

namespace DispatcherWeb.Web.Models.Payment
{
    public class BuyEditionViewModel
    {
        public SubscriptionStartType? SubscriptionStartType { get; set; }

        public EditionSelectDto Edition { get; set; }

        public decimal? AdditionalPrice { get; set; }

        public EditionPaymentType EditionPaymentType { get; set; }

        public List<PaymentGatewayModel> PaymentGateways { get; set; }
    }
}
