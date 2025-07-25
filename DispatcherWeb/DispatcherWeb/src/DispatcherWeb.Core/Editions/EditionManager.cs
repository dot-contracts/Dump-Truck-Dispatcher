using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;

namespace DispatcherWeb.Editions
{
    public class EditionManager : AbpEditionManager
    {
        public const string DefaultEditionName = "Standard";
        public const string LiteEditionName = "Lite";
        public const string PremiumEditionName = "Premium";
        public const string FreeEditionName = "Free";

        public EditionManager(
            IRepository<Edition> editionRepository,
            IAbpZeroFeatureValueStore featureValueStore,
            IUnitOfWorkManager unitOfWorkManager)
            : base(
                editionRepository,
                featureValueStore,
                unitOfWorkManager
            )
        {
        }
    }
}
