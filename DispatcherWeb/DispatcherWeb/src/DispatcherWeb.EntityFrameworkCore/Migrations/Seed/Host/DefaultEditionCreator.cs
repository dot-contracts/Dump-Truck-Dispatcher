using System.Linq;
using Abp.Application.Features;
using DispatcherWeb.Editions;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Features;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Host
{
    public class DefaultEditionCreator
    {
        private readonly DispatcherWebDbContext _context;

        public DefaultEditionCreator(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateEditions();
        }

        private void CreateEditions()
        {
            var defaultEdition = _context.Editions.IgnoreQueryFilters().OfType<SubscribableEdition>().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
            if (defaultEdition == null)
            {
                defaultEdition = new SubscribableEdition { Name = EditionManager.DefaultEditionName, DisplayName = EditionManager.DefaultEditionName };
                _context.Editions.Add(defaultEdition);
                _context.SaveChanges();
            }

            if (defaultEdition.Id > 0)
            {
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.ChatFeature, true);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.TenantToTenantChatFeature, true);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.TenantToHostChatFeature, true);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.AllowPaymentProcessingFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.NumberOfTrucksFeature, 100);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.GpsIntegrationFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.DispatchingFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.QuickbooksFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.AllowLeaseHaulersFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.AllowInvoicingFeature, false);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.DriverProductionPayFeature, false);
            }

            int liteEditionId = CreateEditionIfNotExists(EditionManager.LiteEditionName);
            if (liteEditionId > 0)
            {
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.ChatFeature, true);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.TenantToTenantChatFeature, true);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.TenantToHostChatFeature, true);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.AllowMultiOfficeFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.AllowPaymentProcessingFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.NumberOfTrucksFeature, 10);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.GpsIntegrationFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.DispatchingFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.QuickbooksFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.AllowLeaseHaulersFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.AllowInvoicingFeature, false);
                CreateFeatureIfNotExists(liteEditionId, AppFeatures.DriverProductionPayFeature, false);
            }

            int premiumEditionId = CreateEditionIfNotExists(EditionManager.PremiumEditionName);
            if (premiumEditionId > 0)
            {
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.ChatFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.TenantToTenantChatFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.TenantToHostChatFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.AllowMultiOfficeFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.AllowPaymentProcessingFeature, false);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.NumberOfTrucksFeature, 200);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.GpsIntegrationFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.DispatchingFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.QuickbooksFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.SmsIntegrationFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.AllowLeaseHaulersFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.AllowInvoicingFeature, true);
                CreateFeatureIfNotExists(premiumEditionId, AppFeatures.DriverProductionPayFeature, false);
            }

            int freeEditionId = CreateEditionIfNotExists(EditionManager.FreeEditionName);
            if (freeEditionId > 0)
            {
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.ChatFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.TenantToTenantChatFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.TenantToHostChatFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.AllowMultiOfficeFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.AllowPaymentProcessingFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.NumberOfTrucksFeature, 100);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.GpsIntegrationFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.DispatchingFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.QuickbooksFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.AllowLeaseHaulersFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.AllowInvoicingFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.DriverProductionPayFeature, false);
                CreateFeatureIfNotExists(freeEditionId, AppFeatures.PaidFunctionality, false);
            }
        }

        private void CreateFeatureIfNotExists(int editionId, string featureName, bool isEnabled)
        {
            var defaultEditionChatFeature = _context.EditionFeatureSettings.IgnoreQueryFilters()
                                                        .FirstOrDefault(ef => ef.EditionId == editionId && ef.Name == featureName);

            if (defaultEditionChatFeature == null)
            {
                _context.EditionFeatureSettings.Add(new EditionFeatureSetting
                {
                    Name = featureName,
                    Value = isEnabled.ToString().ToLower(),
                    EditionId = editionId,
                });
            }
        }

        private void CreateFeatureIfNotExists(int editionId, string featureName, int value)
        {
            CreateFeatureIfNotExists(editionId, featureName, value.ToString("N0"));
        }

        private void CreateFeatureIfNotExists(int editionId, string featureName, string value)
        {
            var feature = _context.EditionFeatureSettings.IgnoreQueryFilters()
                                                        .FirstOrDefault(ef => ef.EditionId == editionId && ef.Name == featureName);

            if (feature == null)
            {
                _context.EditionFeatureSettings.Add(new EditionFeatureSetting
                {
                    Name = featureName,
                    Value = value,
                    EditionId = editionId,
                });
            }
        }

        private int CreateEditionIfNotExists(string editionName)
        {
            var edition = _context.Editions.IgnoreQueryFilters().OfType<SubscribableEdition>().FirstOrDefault(e => e.Name == editionName);
            if (edition == null)
            {
                edition = new SubscribableEdition { Name = editionName, DisplayName = editionName };
                _context.Editions.Add(edition);
                _context.SaveChanges();

            }

            return edition.Id;
        }
    }
}
