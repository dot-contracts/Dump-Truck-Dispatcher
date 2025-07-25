using System.Threading.Tasks;

namespace DispatcherWeb.WebPush
{
    public interface IFirebasePushSender
    {
        Task SendAsync(FcmRegistrationTokenDto registrationToken, string jsonPayload);
    }
}
