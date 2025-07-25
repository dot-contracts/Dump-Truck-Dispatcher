using Abp.Auditing;

namespace DispatcherWeb.DriverApp.Account.Dto
{
    public class ResetPasswordInput : SendPasswordResetCodeInput
    {
        public string ResetCode { get; set; }

        [DisableAuditing]
        public string Password { get; set; }
    }
}
