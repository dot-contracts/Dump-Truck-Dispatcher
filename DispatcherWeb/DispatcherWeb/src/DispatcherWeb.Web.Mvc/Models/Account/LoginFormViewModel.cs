namespace DispatcherWeb.Web.Models.Account
{
    public class LoginFormViewModel
    {
        public string SuccessMessage { get; set; }

        public string WarningMessage { get; set; }

        public string UserNameOrEmailAddress { get; set; }

        public bool ForceHostLogin { get; set; }

        public bool IsSelfRegistrationEnabled { get; set; }

        public bool IsTenantSelfRegistrationEnabled { get; set; }

        public bool IsEmailActivationEnabled { get; set; }
    }
}