using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Features;
using DispatcherWeb.Items;

namespace DispatcherWeb.MultiTenancy
{
    public class FeatureMigrator : IFeatureMigrator, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IItemRepository _itemRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly TenantManager _tenantManager;

        public FeatureMigrator(
            IUnitOfWorkManager unitOfWorkManager,
            IItemRepository itemRepository,
            IBackgroundJobManager backgroundJobManager,
            TenantManager tenantManager
        )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _itemRepository = itemRepository;
            _backgroundJobManager = backgroundJobManager;
            _tenantManager = tenantManager;
        }

        public async Task MigrateTenantIfNeeded(int tenantId, IReadOnlyCollection<NameValue> oldFeatureValues)
        {
            await _unitOfWorkManager.Current.SaveChangesAsync();
            var newFeatureValues = await _tenantManager.GetFeatureValuesAsync(tenantId);

            var oldSeparateMaterialAndFreightItems = oldFeatureValues.FirstOrDefault(x => x.Name == AppFeatures.SeparateMaterialAndFreightItems)?.Value;
            var newSeparateMaterialAndFreightItems = newFeatureValues.FirstOrDefault(x => x.Name == AppFeatures.SeparateMaterialAndFreightItems)?.Value;

            if (oldSeparateMaterialAndFreightItems != newSeparateMaterialAndFreightItems)
            {
                await _backgroundJobManager.EnqueueAsync<MigrateToSeparateMaterialAndFreightItemsBackgroundJob, MigrateToSeparateMaterialAndFreightItemsBackgroundJobArgs>(new MigrateToSeparateMaterialAndFreightItemsBackgroundJobArgs
                {
                    TenantIds = new List<int> { tenantId },
                    SeparateMaterialAndFreightItems = newSeparateMaterialAndFreightItems == "true",
                });
            }

            //add other migrations below
        }
    }
}
