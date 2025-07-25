using DispatcherWeb.Localization.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Languages
{
    public class CreateOrEditLanguageModalViewModel : GetLanguageForEditOutput
    {
        public bool IsEditMode => Language.Id.HasValue;

        public CreateOrEditLanguageModalViewModel(GetLanguageForEditOutput output)
        {
            this.Language = output.Language;
            this.LanguageNames = output.LanguageNames;
            this.Flags = output.Flags;
            //output.MapTo(this);
        }
    }
}
