using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.MultiTenancy.Payments.PayPal.Dto;

namespace DispatcherWeb.MultiTenancy.Payments.PayPal
{
    public interface IPayPalPaymentAppService : IApplicationService
    {
        Task ConfirmPayment(long paymentId, string paypalOrderId);

        PayPalConfigurationDto GetConfiguration();
    }
}
