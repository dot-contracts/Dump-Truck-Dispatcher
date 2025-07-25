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
        public class OrderToDateKey : BaseCache<int, ListCacheDateKey, Order>, ISingletonDependency
        {
            public OrderToDateKey(
                BaseCacheDependency baseCacheDependency,
                IRepository<Order, int> repository
            ) : base(baseCacheDependency, repository)
            {
            }

            protected override string CacheNameSuffix => "OrderToDateKey";

            protected override async Task<ListCacheDateKey> GetKeyFromDatabase(IQueryable<Order> queryable)
            {
                return await queryable
                    .Select(x => new ListCacheDateKey
                    {
                        TenantId = x.TenantId,
                        Date = x.DeliveryDate,
                        Shift = x.Shift,
                    })
                    .FirstOrDefaultAsync();
            }

            protected override bool IsKeyValidForEntity(ListCacheDateKey key, Order entity)
            {
                return key != null
                       && entity.DeliveryDate == key.Date
                       && entity.Shift == key.Shift
                       && entity.TenantId == key.TenantId;
            }

            protected override ListCacheDateKey CreateParentKeyForEntity(Order entity)
            {
                return new ListCacheDateKey(entity.TenantId, entity.DeliveryDate, entity.Shift);
            }
        }
    }
}
