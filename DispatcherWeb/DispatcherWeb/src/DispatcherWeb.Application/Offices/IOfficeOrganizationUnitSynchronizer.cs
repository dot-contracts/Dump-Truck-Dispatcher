using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Organizations;

namespace DispatcherWeb.Offices
{
    public interface IOfficeOrganizationUnitSynchronizer
    {
        Task AddUserToOrganizationUnitForOfficeId(long userId, int officeId);
        Task<bool> CanDeleteOffice(EntityDto input);
        Task<bool> CanDeleteOrganizationUnit(EntityDto<long> input);
        Task DeleteOffice(EntityDto input);
        Task DeleteOrganizationUnit(EntityDto<long> input);
        Task<long> GetOrganizationUnitIdForOfficeId(int officeId);
        bool IsAllowedToMoveOrganizationUnit(OrganizationUnit organizationUnit);
        bool IsAllowedToRenameOrganizationUnit(OrganizationUnit organizationUnit);
        Task<bool> IsOrganizationUnitOfficeRelated(OrganizationUnit organizationUnit);
        bool IsRootOrganizationUnitForOffices(OrganizationUnit organizationUnit);
        Task MigrateOfficesForCurrentTenant();
        Task MigrateOfficesForTenant(int tenantId);
        Task UpdateOffice(OrganizationUnit organizationUnit);
        Task UpdateOrganizationUnit(Office office);
    }
}
