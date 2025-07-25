using Abp.Auditing;

namespace DispatcherWeb.Authorization.Accounts.Dto
{
    public class ResetPasswordInput
    {
        public long UserId { get; set; }

        public string ResetCode { get; set; }

        [DisableAuditing]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public string SingleSignIn { get; set; }
    }
}
