namespace DispatcherWeb.Authorization.Accounts.Dto
{
    public class ActivateEmailInput
    {
        public long UserId { get; set; }

        public string ConfirmationCode { get; set; }
    }
}
