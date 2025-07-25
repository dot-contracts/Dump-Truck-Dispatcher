using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Authorization.Accounts.Dto
{
    public class SendEmailActivationLinkInput
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}