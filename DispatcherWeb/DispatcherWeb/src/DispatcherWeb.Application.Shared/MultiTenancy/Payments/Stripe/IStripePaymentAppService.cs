using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.MultiTenancy.Payments.Dto;
using DispatcherWeb.MultiTenancy.Payments.Stripe.Dto;

namespace DispatcherWeb.MultiTenancy.Payments.Stripe
{
    public interface IStripePaymentAppService : IApplicationService
    {
        Task ConfirmPayment(StripeConfirmPaymentInput input);

        StripeConfigurationDto GetConfiguration();

        Task<SubscriptionPaymentDto> GetPaymentAsync(StripeGetPaymentInput input);

        Task<string> CreatePaymentSession(StripeCreatePaymentSessionInput input);
    }
}