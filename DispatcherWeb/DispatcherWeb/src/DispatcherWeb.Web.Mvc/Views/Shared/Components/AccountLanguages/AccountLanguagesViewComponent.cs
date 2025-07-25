using System.Linq;
using System.Threading.Tasks;
using Abp.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Views.Shared.Components.AccountLanguages
{
    public class AccountLanguagesViewComponent : DispatcherWebViewComponent
    {
        private readonly ILanguageManager _languageManager;

        public AccountLanguagesViewComponent(ILanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new LanguageSelectionViewModel
            {
                CurrentLanguage = await _languageManager.GetCurrentLanguageAsync(),
                Languages = (await _languageManager.GetLanguagesAsync()).Where(l => !l.IsDisabled).ToList(),
                CurrentUrl = Request.Path,
            };

            return View(model);
        }
    }
}
