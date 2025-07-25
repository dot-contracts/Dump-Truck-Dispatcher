using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverApp.FcmRegistrationTokens.Dto
{
    public class DeleteInput
    {
        [Required]
        [StringLength(EntityStringFieldLengths.FcmRegistrationToken.Token)]
        public string Token { get; set; }
    }
}
