using System.Threading.Tasks;
using DispatcherWeb.DriverApp.Account.Dto;

namespace DispatcherWeb.DriverApp.Account
{
    public interface IAccountAppService
    {
        Task SendPasswordResetCode(SendPasswordResetCodeInput input);
        Task<Authorization.Accounts.Dto.ResetPasswordOutput> ResetPassword(ResetPasswordInput input);
        Task ValidateTenant(ValidateTenantInput input);
    }
}
