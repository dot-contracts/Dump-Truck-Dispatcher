using System.Globalization;
using System.Threading.Tasks;
using Abp.Localization;
using Abp.Zero;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Localization
{
    public class Localization_Tests : AppTestBase
    {
        [Theory]
        [InlineData("en")]
        [InlineData("en-US")]
        [InlineData("en-GB")]
        public async Task Simple_Localization_Test(string cultureName)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            var manager = Resolve<ILanguageManager>();
            (await manager.GetCurrentLanguageAsync()).Name.ShouldBe("en");

            Resolve<ILocalizationManager>()
                .GetString(AbpZeroConsts.LocalizationSourceName, "Identity.UserNotInRole")
                .ShouldBe("User is not in role '{0}'.");
        }
    }
}
