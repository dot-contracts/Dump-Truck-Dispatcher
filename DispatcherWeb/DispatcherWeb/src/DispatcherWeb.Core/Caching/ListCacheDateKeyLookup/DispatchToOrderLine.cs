using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Dispatching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService
    {
        public class DispatchToOrderLine : BaseCache<int, int?, Dispatch>, ISingletonDependency
        {
            public DispatchToOrderLine(
                BaseCacheDependency baseCacheDependency,
                IRepository<Dispatch, int> repository
            ) : base(baseCacheDependency, repository)
            {
            }

            protected override string CacheNameSuffix => "DispatchToOrderLine";

            protected override async Task<int?> GetKeyFromDatabase(IQueryable<Dispatch> queryable)
            {
                return await queryable
                    .Select(x => (int?)x.OrderLineId)
                    .FirstOrDefaultAsync();
            }

            protected override bool IsKeyValidForEntity(int? key, Dispatch entity)
            {
                return entity.OrderLineId == key;
            }

            protected override int? CreateParentKeyForEntity(Dispatch entity)
            {
                return entity.OrderLineId;
            }
        }
    }
}
