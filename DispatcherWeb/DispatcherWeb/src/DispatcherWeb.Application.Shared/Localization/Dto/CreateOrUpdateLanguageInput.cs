using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Localization.Dto
{
    public class CreateOrUpdateLanguageInput
    {
        [Required]
        public ApplicationLanguageEditDto Language { get; set; }
    }
}