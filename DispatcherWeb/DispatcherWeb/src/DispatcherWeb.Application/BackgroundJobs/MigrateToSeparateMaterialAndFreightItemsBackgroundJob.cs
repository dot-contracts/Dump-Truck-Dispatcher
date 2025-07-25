using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using DispatcherWeb.Items;

namespace DispatcherWeb.BackgroundJobs
{
    public class MigrateToSeparateMaterialAndFreightItemsBackgroundJob : AsyncBackgroundJob<MigrateToSeparateMaterialAndFreightItemsBackgroundJobArgs>, ITransientDependency
    {
        private readonly IItemRepository _itemRepository;

        public MigrateToSeparateMaterialAndFreightItemsBackgroundJob(
            IItemRepository itemRepository
        )
        {
            _itemRepository = itemRepository;
        }

        public override async Task ExecuteAsync(MigrateToSeparateMaterialAndFreightItemsBackgroundJobArgs args)
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await _itemRepository.MigrateToSeparateMaterialAndFreightItems(args.TenantIds, args.SeparateMaterialAndFreightItems);
            });
        }
    }
}
