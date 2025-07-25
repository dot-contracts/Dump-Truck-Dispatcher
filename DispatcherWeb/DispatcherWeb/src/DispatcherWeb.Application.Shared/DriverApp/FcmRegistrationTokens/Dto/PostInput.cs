using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverApp.FcmRegistrationTokens.Dto
{
    public class PostInput
    {
        [Required]
        [StringLength(EntityStringFieldLengths.FcmRegistrationToken.Token)]
        public string Token { get; set; }

        [Required]
        public MobilePlatform MobilePlatform { get; set; }

        public int? Version { get; set; }
    }
}
