using System.Threading.Tasks;

namespace DispatcherWeb.Sms
{
    public interface ISmsAppService
    {
        Task SetSmsStatus(string sid, string smsStatus);
    }
}