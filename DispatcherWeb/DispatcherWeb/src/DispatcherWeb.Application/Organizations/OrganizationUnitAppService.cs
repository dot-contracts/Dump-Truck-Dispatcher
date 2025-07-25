using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Offices;
using DispatcherWeb.Organizations.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Organizations
{
    [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits)]
    public class OrganizationUnitAppService : DispatcherWebAppServiceBase, IOrganizationUnitAppService
    {
        private readonly OrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly RoleManager _roleManager;

        public OrganizationUnitAppService(
            OrganizationUnitManager organizationUnitManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            RoleManager roleManager,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
        {
            _organizationUnitManager = organizationUnitManager;
            _organizationUnitRepository = organizationUnitRepository;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _roleManager = roleManager;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
        }

        public async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnits()
        {
            var organizationUnitMemberCounts = await (await _userOrganizationUnitRepository.GetQueryAsync())
                .GroupBy(x => x.OrganizationUnitId)
                .Select(groupedUsers => new
                {
                    organizationUnitId = groupedUsers.Key,
                    count = groupedUsers.Count(),
                }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);

            var organizationUnitRoleCounts = await (await _organizationUnitRoleRepository.GetQueryAsync())
                .GroupBy(x => x.OrganizationUnitId)
                .Select(groupedRoles => new
                {
                    organizationUnitId = groupedRoles.Key,
                    count = groupedRoles.Count(),
                }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);

            var organizationUnits = await (await _organizationUnitRepository.GetQueryAsync())
                .Select(ou => new OrganizationUnitDto
                {
                    Id = ou.Id,
                    ParentId = ou.ParentId,
                    Code = ou.Code,
                    DisplayName = ou.DisplayName,
                }).ToListAsync();

            foreach (var organizationUnit in organizationUnits)
            {
                organizationUnit.MemberCount = organizationUnitMemberCounts.TryGetValue(organizationUnit.Id, out var count) ? count : 0;
                organizationUnit.RoleCount = organizationUnitRoleCounts.TryGetValue(organizationUnit.Id, out var roleCount) ? roleCount : 0;
            }

            return new ListResultDto<OrganizationUnitDto>(organizationUnits);
        }

        public async Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsers(GetOrganizationUnitUsersInput input)
        {
            var query = from ouUser in await _userOrganizationUnitRepository.GetQueryAsync()
                        join ou in await _organizationUnitRepository.GetQueryAsync() on ouUser.OrganizationUnitId equals ou.Id
                        join user in await UserManager.GetQueryAsync() on ouUser.UserId equals user.Id
                        where ouUser.OrganizationUnitId == input.Id
                        select new
                        {
                            ouUser,
                            user,
                        };

            var totalCount = await query.CountAsync();
            var items = await query
                .Select(x => new OrganizationUnitUserListDto
                {
                    Id = x.user.Id,
                    Name = x.user.Name,
                    Surname = x.user.Surname,
                    UserName = x.user.UserName,
                    EmailAddress = x.user.EmailAddress,
                    ProfilePictureId = x.user.ProfilePictureId,
                    AddedTime = x.ouUser.CreationTime,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OrganizationUnitUserListDto>(totalCount, items);
        }

        public async Task<PagedResultDto<OrganizationUnitRoleListDto>> GetOrganizationUnitRoles(GetOrganizationUnitRolesInput input)
        {
            var query = from ouRole in await _organizationUnitRoleRepository.GetQueryAsync()
                        join ou in await _organizationUnitRepository.GetQueryAsync() on ouRole.OrganizationUnitId equals ou.Id
                        join role in await _roleManager.GetQueryAsync() on ouRole.RoleId equals role.Id
                        where ouRole.OrganizationUnitId == input.Id
                        select new
                        {
                            ouRole,
                            role,
                        };

            var totalCount = await query.CountAsync();
            var items = await query
                .Select(x => new OrganizationUnitRoleListDto
                {
                    Id = x.role.Id,
                    DisplayName = x.role.DisplayName,
                    Name = x.role.Name,
                    AddedTime = x.ouRole.CreationTime,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OrganizationUnitRoleListDto>(totalCount, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task<OrganizationUnitDto> CreateOrganizationUnit(CreateOrganizationUnitInput input)
        {
            var organizationUnit = new OrganizationUnit(await AbpSession.GetTenantIdOrNullAsync(), input.DisplayName, input.ParentId);

            await _organizationUnitManager.CreateAsync(organizationUnit);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _officeOrganizationUnitSynchronizer.UpdateOffice(organizationUnit);

            return new OrganizationUnitDto
            {
                Id = organizationUnit.Id,
                ParentId = organizationUnit.ParentId,
                Code = organizationUnit.Code,
                DisplayName = organizationUnit.DisplayName,
                MemberCount = await _userOrganizationUnitRepository.CountAsync(uou => uou.OrganizationUnitId == organizationUnit.Id),
                RoleCount = await _organizationUnitRoleRepository.CountAsync(uou => uou.OrganizationUnitId == organizationUnit.Id),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task<OrganizationUnitDto> UpdateOrganizationUnit(UpdateOrganizationUnitInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);

            organizationUnit.DisplayName = input.DisplayName;

            await _organizationUnitManager.UpdateAsync(organizationUnit);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _officeOrganizationUnitSynchronizer.UpdateOffice(organizationUnit);

            return await CreateOrganizationUnitDto(organizationUnit);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task<OrganizationUnitDto> MoveOrganizationUnit(MoveOrganizationUnitInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);
            if (!_officeOrganizationUnitSynchronizer.IsAllowedToMoveOrganizationUnit(organizationUnit))
            {
                throw new UserFriendlyException(L("YouCannotMoveThisOrganizationUnit"));
            }

            await _organizationUnitManager.MoveAsync(input.Id, input.NewParentId);

            return await CreateOrganizationUnitDto(
                await _organizationUnitRepository.GetAsync(input.Id)
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task DeleteOrganizationUnit(EntityDto<long> input)
        {
            await _officeOrganizationUnitSynchronizer.DeleteOrganizationUnit(input);
        }


        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers)]
        public async Task RemoveUserFromOrganizationUnit(UserToOrganizationUnitInput input)
        {
            await UserManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles)]
        public async Task RemoveRoleFromOrganizationUnit(RoleToOrganizationUnitInput input)
        {
            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers)]
        public async Task AddUsersToOrganizationUnit(UsersToOrganizationUnitInput input)
        {
            foreach (var userId in input.UserIds)
            {
                await UserManager.AddToOrganizationUnitAsync(userId, input.OrganizationUnitId);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles)]
        public async Task AddRolesToOrganizationUnit(RolesToOrganizationUnitInput input)
        {
            foreach (var roleId in input.RoleIds)
            {
                await _roleManager.AddToOrganizationUnitAsync(roleId, input.OrganizationUnitId, await AbpSession.GetTenantIdOrNullAsync());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree)]
        public async Task<OrganizationUnitEditDto> GetOrganizationUnitForEdit(long id)
        {
            return await (await _organizationUnitRepository.GetQueryAsync())
                .Where(x => x.Id == id)
                .Select(x => new OrganizationUnitEditDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                }).FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers)]
        public async Task<PagedResultDto<NameValueDto>> FindUsers(FindOrganizationUnitUsersInput input)
        {
            var userIdsInOrganizationUnit = (await _userOrganizationUnitRepository.GetQueryAsync())
                .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
                .Select(uou => uou.UserId);

            var query = (await UserManager.GetQueryAsync())
                .Where(u => !userIdsInOrganizationUnit.Contains(u.Id))
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    u =>
                        u.Name.Contains(input.Filter)
                        || u.Surname.Contains(input.Filter)
                        || u.UserName.Contains(input.Filter)
                        || u.EmailAddress.Contains(input.Filter)
                );

            var userCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Surname)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<NameValueDto>(
                userCount,
                users.Select(u =>
                    new NameValueDto(
                        u.FullName + " (" + u.EmailAddress + ")",
                        u.Id.ToString()
                    )
                ).ToList()
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles)]
        public async Task<PagedResultDto<NameValueDto>> FindRoles(FindOrganizationUnitRolesInput input)
        {
            var roleIdsInOrganizationUnit = (await _organizationUnitRoleRepository.GetQueryAsync())
                .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
                .Select(uou => uou.RoleId);

            var query = (await _roleManager.GetQueryAsync())
                .Where(u => !roleIdsInOrganizationUnit.Contains(u.Id))
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    u =>
                        u.DisplayName.Contains(input.Filter)
                        || u.Name.Contains(input.Filter)
                );

            var roleCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.DisplayName)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<NameValueDto>(
                roleCount,
                users.Select(u =>
                    new NameValueDto(
                        u.DisplayName,
                        u.Id.ToString()
                    )
                ).ToList()
            );
        }

        private async Task<OrganizationUnitDto> CreateOrganizationUnitDto(OrganizationUnit organizationUnit)
        {
            var dto = new OrganizationUnitDto
            {
                MemberCount = await _userOrganizationUnitRepository.CountAsync(uou => uou.OrganizationUnitId == organizationUnit.Id),
                ParentId = organizationUnit.ParentId,
                Code = organizationUnit.Code,
                RoleCount = await _organizationUnitRoleRepository.CountAsync(uou => uou.OrganizationUnitId == organizationUnit.Id),
                DisplayName = organizationUnit.DisplayName,
                Id = organizationUnit.Id,
            };

            return dto;
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
        public async Task<string> MigrateOfficesForAllTenants()
        {
            var tenantIds = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                return await (await TenantManager.GetQueryAsync())
                    .Select(x => x.Id)
                    .ToListAsync();
            });

            var result = $"started at {Clock.Now:s}";
            try
            {
                foreach (var tenantId in tenantIds)
                {
                    await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                    {
                        using (CurrentUnitOfWork.SetTenantId(tenantId))
                        using (Session.Use(tenantId, Session.UserId))
                        {
                            await _officeOrganizationUnitSynchronizer.MigrateOfficesForCurrentTenant();
                        }
                    }, new UnitOfWorkOptions
                    {
                        Timeout = TimeSpan.FromMinutes(60),
                    });
                    result += $"\r\n migrated for tenant {tenantId} at {Clock.Now:s}";
                }
                result += $"\r\n done at {Clock.Now:s}";
            }
            catch (Exception ex)
            {
                result += "\r\n" + ex.Message;
            }

            return result;
        }
    }
}
