using DispatcherWeb.Editions;
using DispatcherWeb.MultiTenancy.Payments.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.SubscriptionManagement
{
    public class PaymentResultViewModel : SubscriptionPaymentDto
    {
        public EditionPaymentType EditionPaymentType { get; set; }
    }
}
