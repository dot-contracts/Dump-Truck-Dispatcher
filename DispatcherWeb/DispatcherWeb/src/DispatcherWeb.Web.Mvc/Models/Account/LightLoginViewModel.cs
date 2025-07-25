using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Web.Models.Account
{
    public class LightLoginViewModel : LoginViewModel
    {
        [Required]
        public string TenancyName { get; set; }
    }
}
