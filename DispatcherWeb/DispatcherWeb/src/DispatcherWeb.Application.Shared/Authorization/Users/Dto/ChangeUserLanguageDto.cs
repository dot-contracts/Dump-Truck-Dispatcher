using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Authorization.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
