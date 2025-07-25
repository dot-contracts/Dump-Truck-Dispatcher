using System.Collections.Generic;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Payments;

namespace DispatcherWeb.Web.Models.Payment
{
    public class ExtendEditionViewModel
    {
        public EditionSelectDto Edition { get; set; }

        public List<PaymentGatewayModel> PaymentGateways { get; set; }
    }
}