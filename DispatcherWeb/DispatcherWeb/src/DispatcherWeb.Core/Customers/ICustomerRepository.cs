using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Customers
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task MergeCustomersAsync(List<int> recordIds, int mainRecordId, int? tenantId);
        Task MergeCustomerContactsAsync(List<int> recordIds, int mainRecordId, int? tenantId);
    }
}
