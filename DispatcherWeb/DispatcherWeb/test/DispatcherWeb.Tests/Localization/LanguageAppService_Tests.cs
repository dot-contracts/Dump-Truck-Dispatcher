using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Localization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Localization;
using DispatcherWeb.Localization.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Localization
{
    public class LanguageAppService_Tests : AppTestBase
    {
        private readonly ILanguageAppService _languageAppService;
        private readonly IApplicationLanguageManager _languageManager;

        public LanguageAppService_Tests()
        {
            var appConfigurationAccessor = Resolve<IAppConfigurationAccessor>();

            if (appConfigurationAccessor.Configuration.IsMultitenancyEnabled())
            {
                LoginAsHostAdmin();
            }
            else
            {
                LoginAsDefaultTenantAdmin();
            }

            _languageAppService = Resolve<ILanguageAppService>();
            _languageManager = Resolve<IApplicationLanguageManager>();
        }

        [Fact(Skip = "Language functionality was temporarily hardcoded")]
        public async Task SetDefaultLanguage()
        {
            //Arrange
            var currentLanguages = await _languageManager.GetLanguagesAsync();
            var randomLanguage = RandomHelper.GetRandomOf(currentLanguages.ToArray());

            //Act
            await _languageAppService.SetDefaultLanguage(
                new SetDefaultLanguageInput
                {
                    Name = randomLanguage.Name,
                });

            //Assert
            var defaultLanguage = await _languageManager.GetDefaultLanguageOrNullAsync(Session.TenantId);

            randomLanguage.ShouldBe(defaultLanguage);
        }
    }
}
