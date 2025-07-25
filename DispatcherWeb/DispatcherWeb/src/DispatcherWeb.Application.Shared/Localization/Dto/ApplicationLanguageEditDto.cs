using System.ComponentModel.DataAnnotations;
using Abp.Localization;

namespace DispatcherWeb.Localization.Dto
{
    public class ApplicationLanguageEditDto
    {
        public virtual int? Id { get; set; }

        [Required]
        [StringLength(ApplicationLanguage.MaxNameLength)]
        public virtual string Name { get; set; }

        [StringLength(ApplicationLanguage.MaxIconLength)]
        public virtual string Icon { get; set; }

        public bool IsEnabled { get; set; }
    }
}
