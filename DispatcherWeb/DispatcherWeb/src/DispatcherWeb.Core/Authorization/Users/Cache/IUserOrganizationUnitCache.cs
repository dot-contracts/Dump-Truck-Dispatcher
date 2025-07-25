using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public interface IUserOrganizationUnitCache
    {
        IOrganizationUnitCache OrganizationUnitCache { get; }

        Task<List<UserOrganizationUnitCacheItem>> GetUserOrganizationUnitsAsync();
        Task<List<UserOrganizationUnitCacheItem>> GetUserOrganizationUnitsAsync(long userId);
        Task<bool> HasAccessToAllOffices();
        Task<bool> HasAccessToAllOffices(long userId, int tenantId);
    }
}
