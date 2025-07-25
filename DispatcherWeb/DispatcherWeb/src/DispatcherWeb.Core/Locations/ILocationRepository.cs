using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Locations
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task MergeLocationsAsync(List<int> recordIds, int mainRecordId, int? tenantId);
        Task MergeLocationContactsAsync(List<int> recordIds, int mainRecordId, int? tenantId);
    }
}
