using System.Linq;
using DispatcherWeb.Editions;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Features;
using DispatcherWeb.Migrations.Seed.Host;
using Shouldly;

namespace DispatcherWeb.Tests.Editions
{
    public class DefaultEditionCreator_Tests : AppTestBase
    {
        public DefaultEditionCreator_Tests()
        {

        }

        [MultiTenantFact]
        public void Create_Should_Seed_Editions()
        {
            var dbContext = Resolve<DispatcherWebDbContext>();
            var defaultEditionCreator = new DefaultEditionCreator(dbContext);

            defaultEditionCreator.Create();

            UsingDbContext(context =>
            {
                var standardEditon = context.Editions.FirstOrDefault(e => e.DisplayName == EditionManager.DefaultEditionName);
                standardEditon.ShouldNotBeNull();

                CheckFeatureValue(context, standardEditon.Id, AppFeatures.ChatFeature, "true");
                CheckFeatureValue(context, standardEditon.Id, AppFeatures.TenantToTenantChatFeature, "true");
                CheckFeatureValue(context, standardEditon.Id, AppFeatures.TenantToHostChatFeature, "true");
                //CheckFeatureValue(context, standardEditon.Id, AppFeatures.AllowMultiOfficeFeature, "false");

                var liteEditon = context.Editions.FirstOrDefault(e => e.DisplayName == EditionManager.LiteEditionName);
                liteEditon.ShouldNotBeNull();

                CheckFeatureValue(context, liteEditon.Id, AppFeatures.ChatFeature, "true");
                CheckFeatureValue(context, liteEditon.Id, AppFeatures.TenantToTenantChatFeature, "true");
                CheckFeatureValue(context, liteEditon.Id, AppFeatures.TenantToHostChatFeature, "true");
                //CheckFeatureValue(context, liteEditon.Id, AppFeatures.AllowMultiOfficeFeature, "false");

                var premiumEditon = context.Editions.FirstOrDefault(e => e.DisplayName == EditionManager.PremiumEditionName);
                premiumEditon.ShouldNotBeNull();

                CheckFeatureValue(context, premiumEditon.Id, AppFeatures.ChatFeature, "true");
                CheckFeatureValue(context, premiumEditon.Id, AppFeatures.TenantToTenantChatFeature, "true");
                CheckFeatureValue(context, premiumEditon.Id, AppFeatures.TenantToHostChatFeature, "true");
                CheckFeatureValue(context, premiumEditon.Id, AppFeatures.AllowMultiOfficeFeature, "true");
            });
        }

        private void CheckFeatureValue(DispatcherWebDbContext context, int editionId, string featureName, string expectedFeatureValue)
        {
            var actualFeatureValue = context.EditionFeatureSettings.FirstOrDefault(s => s.EditionId == editionId && s.Name == featureName);
            actualFeatureValue.ShouldNotBe(null);
            actualFeatureValue.Value.ShouldBe(expectedFeatureValue);
        }

    }
}
