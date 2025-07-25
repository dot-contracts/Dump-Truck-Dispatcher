using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Web.Models.Account
{
    public class SendPasswordResetLinkViewModel
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}