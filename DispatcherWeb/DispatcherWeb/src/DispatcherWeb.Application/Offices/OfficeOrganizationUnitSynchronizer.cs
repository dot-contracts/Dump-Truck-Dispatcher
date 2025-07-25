using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Organizations;
using Abp.UI;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Offices
{
    public class OfficeOrganizationUnitSynchronizer : DispatcherWebDomainServiceBase, IOfficeOrganizationUnitSynchronizer
    {
        private readonly OrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;

        public OfficeOrganizationUnitSynchronizer(
            OrganizationUnitManager organizationUnitManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<Office> officeRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository
        )
        {
            _organizationUnitManager = organizationUnitManager;
            _organizationUnitRepository = organizationUnitRepository;
            _officeRepository = officeRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
        }

        public async Task UpdateOrganizationUnit(Office office)
        {
            var organizationUnit = await GetOrCreateLinkedOrganizationUnit(office);
            var officeName = TruncateOfficeNameIfNeeded(office.Name);

            if (officeName != organizationUnit.DisplayName)
            {
                var rootId = await GetRootOrganizationUnitIdForOffices();
                if (await (await _organizationUnitRepository.GetQueryAsync())
                    .AnyAsync(x => x.ParentId == rootId && x.Id != organizationUnit.Id && x.DisplayName == officeName))
                {
                    throw new UserFriendlyException("An organization unit with the same name already exists");
                }

                organizationUnit.DisplayName = officeName;
            }
        }

        public async Task UpdateOffice(OrganizationUnit organizationUnit)
        {
            if (!await IsOrganizationUnitOfficeRelated(organizationUnit))
            {
                return;
            }
            if (IsRootOrganizationUnitForOffices(organizationUnit))
            {
                return;
            }
            var office = await GetLinkedOfficeOrDefault(organizationUnit);
            if (office == null)
            {
                return;
            }

            var officeName = TruncateOfficeNameIfNeeded(office.Name);
            if (officeName != organizationUnit.DisplayName)
            {
                if (await (await _officeRepository.GetQueryAsync())
                    .AnyAsync(x => x.Id != office.Id && x.Name == organizationUnit.DisplayName))
                {
                    throw new UserFriendlyException("An office with the same name already exists");
                }
                office.Name = organizationUnit.DisplayName;
            }
        }

        private async Task<OrganizationUnit> GetOrCreateLinkedOrganizationUnit(Office office)
        {
            OrganizationUnit organizationUnit;
            if (office.OrganizationUnitId != null)
            {
                organizationUnit = await _organizationUnitRepository.FirstOrDefaultAsync(office.OrganizationUnitId.Value);
                if (organizationUnit != null)
                {
                    return organizationUnit;
                }
                else
                {
                    Logger.Warn($"Organization unit for office {office.Id} and OU id {office.OrganizationUnitId} wasn't found");
                }
            }

            var rootId = await GetRootOrganizationUnitIdForOffices();
            var officeName = TruncateOfficeNameIfNeeded(office.Name);
            var existingOrganizationUnit = await (await _organizationUnitRepository.GetQueryAsync())
                .Where(x => x.DisplayName == officeName && x.ParentId == rootId)
                .FirstOrDefaultAsync();

            if (existingOrganizationUnit != null)
            {
                organizationUnit = existingOrganizationUnit;
            }
            else
            {
                organizationUnit = new OrganizationUnit(CurrentUnitOfWork.GetTenantId(), officeName, rootId);
                await _organizationUnitManager.CreateAsync(organizationUnit);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            office.OrganizationUnitId = organizationUnit.Id;
            return organizationUnit;
        }

        private async Task<Office> GetLinkedOfficeOrDefault(OrganizationUnit organizationUnit)
        {
            if (!await IsOrganizationUnitOfficeRelated(organizationUnit))
            {
                return null;
            }
            if (IsRootOrganizationUnitForOffices(organizationUnit))
            {
                return null;
            }
            var office = await (await _officeRepository.GetQueryAsync()).FirstOrDefaultAsync(x => x.OrganizationUnitId == organizationUnit.Id);
            if (office != null)
            {
                return office;
            }

            office = (await _officeRepository.GetQueryAsync()).FirstOrDefault(x => x.Name == organizationUnit.DisplayName);
            if (office != null)
            {
                office.OrganizationUnitId = organizationUnit.Id;
                return office;
            }

            return null;
        }

        private async Task<long> GetRootOrganizationUnitIdForOffices()
        {
            var existingRoot = await (await _organizationUnitRepository.GetQueryAsync())
                .Where(x => x.ParentId == null && x.DisplayName == DispatcherWebConsts.OfficesOrganizationUnitName)
                .Select(x => new
                {
                    x.Id,
                })
                .FirstOrDefaultAsync();

            if (existingRoot != null)
            {
                return existingRoot.Id;
            }

            var newRoot = new OrganizationUnit(CurrentUnitOfWork.GetTenantId(), DispatcherWebConsts.OfficesOrganizationUnitName, null);
            await _organizationUnitManager.CreateAsync(newRoot);
            await CurrentUnitOfWork.SaveChangesAsync();
            return newRoot.Id;
        }

        public async Task<bool> CanDeleteOffice(EntityDto input)
        {
            var office = await _officeRepository.GetAsync(input.Id);
            if (!await CanDeleteOfficeInternal(office))
            {
                return false;
            }

            var organizationUnit = await GetOrCreateLinkedOrganizationUnit(office);
            return await CanDeleteOrganizationUnitInternal(organizationUnit);
        }

        public async Task<bool> CanDeleteOrganizationUnit(EntityDto<long> input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);
            if (IsRootOrganizationUnitForOffices(organizationUnit))
            {
                return false;
            }
            if (!await CanDeleteOrganizationUnitInternal(organizationUnit))
            {
                return false;
            }
            if (!await IsOrganizationUnitOfficeRelated(organizationUnit))
            {
                return true;
            }
            var office = await GetLinkedOfficeOrDefault(organizationUnit);
            return office == null || await CanDeleteOfficeInternal(office);
        }

        private async Task<bool> CanDeleteOfficeInternal(Office office)
        {
            if (office != null)
            {
                var officeRecord = await (await _officeRepository.GetQueryAsync())
                    .Where(x => x.Id == office.Id)
                    .Select(x => new
                    {
                        HasTrucks = x.Trucks.Any(),
                        HasUsers = x.Users.Any(),
                        HasOrders = x.Orders.Any(),
                        HasQuotes = x.Quotes.Any(),
                    })
                    .SingleAsync();

                if (officeRecord.HasTrucks
                    || officeRecord.HasUsers
                    || officeRecord.HasOrders
                    || officeRecord.HasQuotes
                    )
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CanDeleteOrganizationUnitInternal(OrganizationUnit organizationUnit)
        {
            if (await (await _userOrganizationUnitRepository.GetQueryAsync()).AnyAsync(x => x.OrganizationUnitId == organizationUnit.Id)
                || await (await _organizationUnitRoleRepository.GetQueryAsync()).AnyAsync(x => x.OrganizationUnitId == organizationUnit.Id))
            {
                return false;
            }

            if (organizationUnit.ParentId == null)
            {
                var rootId = await GetRootOrganizationUnitIdForOffices();
                if (organizationUnit.Id == rootId)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task DeleteOrganizationUnit(EntityDto<long> input)
        {
            var canDelete = await CanDeleteOrganizationUnit(input);
            if (!canDelete)
            {
                throw new UserFriendlyException(L("UnableToDeleteSelectedRowWithAssociatedData"));
            }
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);
            var office = await GetLinkedOfficeOrDefault(organizationUnit);
            await DeleteOrganizationUnitInternal(organizationUnit);
            if (office != null)
            {
                await DeleteOfficeInternal(office);
            }
        }

        public async Task DeleteOffice(EntityDto input)
        {
            var canDelete = await CanDeleteOffice(input);
            if (!canDelete)
            {
                throw new UserFriendlyException(L("UnableToDeleteSelectedRowWithAssociatedData"));
            }
            var office = await _officeRepository.GetAsync(input.Id);
            var organizationUnit = await GetOrCreateLinkedOrganizationUnit(office);
            await DeleteOfficeInternal(office);
            await DeleteOrganizationUnitInternal(organizationUnit);
        }

        private async Task DeleteOfficeInternal(Office office)
        {
            await _officeRepository.DeleteAsync(office);
        }

        private async Task DeleteOrganizationUnitInternal(OrganizationUnit organizationUnit)
        {
            await _userOrganizationUnitRepository.DeleteAsync(x => x.OrganizationUnitId == organizationUnit.Id);
            await _organizationUnitRoleRepository.DeleteAsync(x => x.OrganizationUnitId == organizationUnit.Id);
            await _organizationUnitManager.DeleteAsync(organizationUnit.Id);
        }

        public bool IsRootOrganizationUnitForOffices(OrganizationUnit organizationUnit)
        {
            return organizationUnit.ParentId == null && organizationUnit.DisplayName == DispatcherWebConsts.OfficesOrganizationUnitName;
        }

        public async Task<bool> IsOrganizationUnitOfficeRelated(OrganizationUnit organizationUnit)
        {
            var office = await (await _officeRepository.GetQueryAsync()).FirstOrDefaultAsync(x => x.OrganizationUnitId == organizationUnit.Id);
            if (office != null)
            {
                return true;
            }

            var officesRootId = await GetRootOrganizationUnitIdForOffices();
            return organizationUnit.ParentId == officesRootId;
        }

        public bool IsAllowedToRenameOrganizationUnit(OrganizationUnit organizationUnit)
        {
            return !IsRootOrganizationUnitForOffices(organizationUnit);
        }

        public bool IsAllowedToMoveOrganizationUnit(OrganizationUnit organizationUnit)
        {
            return !IsRootOrganizationUnitForOffices(organizationUnit);
        }

        private string TruncateOfficeNameIfNeeded(string officeName)
        {
            if (officeName == null || officeName.Length <= OrganizationUnit.MaxDisplayNameLength)
            {
                return officeName;
            }
            return officeName.Left(OrganizationUnit.MaxDisplayNameLength);
        }

        public async Task MigrateOfficesForTenant(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await MigrateOfficesForCurrentTenant();
            }
        }

        public async Task MigrateOfficesForCurrentTenant()
        {
            var offices = await (await _officeRepository.GetQueryAsync()).ToListAsync();
            var officeOuDictionary = new Dictionary<int, OrganizationUnit>();
            foreach (var office in offices)
            {
                var organizationUnit = await GetOrCreateLinkedOrganizationUnit(office);
                officeOuDictionary.Add(office.Id, organizationUnit);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            var users = await (await UserManager.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.OfficeId,
                })
                .ToListAsync();
            foreach (var userGroup in users.GroupBy(x => x.OfficeId))
            {
                if (userGroup.Key == null || !officeOuDictionary.ContainsKey(userGroup.Key.Value))
                {
                    continue;
                }

                var organizationUnit = officeOuDictionary[userGroup.Key.Value];
                foreach (var user in userGroup)
                {
                    await UserManager.AddToOrganizationUnitAsync(user.Id, organizationUnit.Id);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<long> GetOrganizationUnitIdForOfficeId(int officeId)
        {
            //this is a separate function in case we need to cache the result later
            var office = await _officeRepository.GetAsync(officeId);
            var organizationUnit = await GetOrCreateLinkedOrganizationUnit(office);
            return organizationUnit.Id;
        }

        public async Task AddUserToOrganizationUnitForOfficeId(long userId, int officeId)
        {
            var organizationUnitId = await GetOrganizationUnitIdForOfficeId(officeId);
            await UserManager.AddToOrganizationUnitAsync(userId, organizationUnitId);
        }
    }
}
