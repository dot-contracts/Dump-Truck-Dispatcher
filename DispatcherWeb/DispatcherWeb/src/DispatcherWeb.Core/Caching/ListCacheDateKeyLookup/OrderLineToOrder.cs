using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService
    {
        public class OrderLineToOrder : BaseCache<int, int?, OrderLine>, ISingletonDependency
        {
            public OrderLineToOrder(
                BaseCacheDependency baseCacheDependency,
                IRepository<OrderLine, int> repository
            ) : base(baseCacheDependency, repository)
            {
            }

            protected override string CacheNameSuffix => "OrderLineToOrder";

            protected override async Task<int?> GetKeyFromDatabase(IQueryable<OrderLine> queryable)
            {
                return await queryable
                    .Select(x => (int?)x.OrderId)
                    .FirstOrDefaultAsync();
            }

            protected override bool IsKeyValidForEntity(int? key, OrderLine entity)
            {
                return entity.OrderId == key;
            }

            protected override int? CreateParentKeyForEntity(OrderLine entity)
            {
                return entity.OrderId;
            }
        }
    }
}
