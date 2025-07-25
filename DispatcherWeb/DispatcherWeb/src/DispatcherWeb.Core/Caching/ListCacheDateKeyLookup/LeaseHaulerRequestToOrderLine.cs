using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.LeaseHaulerRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService
    {
        public class LeaseHaulerRequestToOrderLine : BaseCache<int, int?, LeaseHaulerRequest>, ISingletonDependency
        {
            public LeaseHaulerRequestToOrderLine(
                BaseCacheDependency baseCacheDependency,
                IRepository<LeaseHaulerRequest, int> repository
            ) : base(baseCacheDependency, repository)
            {
            }

            protected override string CacheNameSuffix => "LeaseHaulerRequestToOrderLine";

            protected override async Task<int?> GetKeyFromDatabase(IQueryable<LeaseHaulerRequest> queryable)
            {
                return await queryable
                    .Select(x => (int?)x.OrderLineId)
                    .FirstOrDefaultAsync();
            }

            protected override bool IsKeyValidForEntity(int? key, LeaseHaulerRequest entity)
            {
                return entity.OrderLineId == key;
            }

            protected override int? CreateParentKeyForEntity(LeaseHaulerRequest entity)
            {
                return entity.OrderLineId;
            }
        }
    }
}
