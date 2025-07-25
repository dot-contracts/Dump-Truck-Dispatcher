using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Items
{
    public interface IItemRepository : IRepository<Item>
    {
        Task MergeItemsAsync(List<int> recordIds, int mainRecordId, int? tenantId);
        Task MigrateToSeparateMaterialAndFreightItems(List<int> tenantIds, bool separateMaterialAndFreightItems);
    }
}
