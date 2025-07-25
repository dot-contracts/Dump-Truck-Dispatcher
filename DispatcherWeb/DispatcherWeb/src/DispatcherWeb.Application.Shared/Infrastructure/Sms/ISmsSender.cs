using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Sms.Dto;

namespace DispatcherWeb.Infrastructure.Sms
{
    public interface ISmsSender
    {
        Task<SmsSendResult> SendAsync(SmsSendInput input);
    }
}