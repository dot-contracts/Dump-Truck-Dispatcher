using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public interface IOrganizationUnitCache
    {
        Task<List<OrganizationUnitCacheItem>> GetAllOrganizationUnitsAsync();
        Task<List<OrganizationUnitCacheItem>> GetAllOrganizationUnitsAsync(int? tenantId);
        Task<List<OrganizationUnitCacheItem>> GetOfficeBasedOrganizationUnitsAsync();
        Task<List<OrganizationUnitCacheItem>> GetOfficeBasedOrganizationUnitsAsync(int? tenantId);
    }
}
