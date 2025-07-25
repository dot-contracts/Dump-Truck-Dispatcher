using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.OrderPayments.Dto;
using DispatcherWeb.Payments.Dto;

namespace DispatcherWeb.OrderPayments
{
    public interface IOrderPaymentAppService : IApplicationService
    {
        Task<AuthorizeOrderChargeDto> GetAuthorizeOrderChargeModel(EntityDto input);
        Task<CaptureOrderAuthorizationDto> GetCaptureOrderAuthorizationModel(EntityDto input);
        Task<AuthorizeOrderChargeResult> AuthorizeOrderCharge(AuthorizeOrderChargeDto input);
        Task CancelOrderAuthorization(EntityDto input);
        Task<CaptureAuthorizationResult> CaptureOrderAuthorization(CaptureOrderAuthorizationDto input);
        Task RefundOrderPayment(EntityDto input);
    }
}
