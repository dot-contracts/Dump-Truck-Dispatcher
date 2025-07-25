using System.Threading.Tasks;

namespace DispatcherWeb.Security.Recaptcha
{
    public interface IRecaptchaValidator
    {
        Task ValidateAsync(string captchaResponse);
    }
}