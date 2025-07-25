using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Permissions.Dto;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Authorization.Users.Exporting;
using DispatcherWeb.Chat;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulerUsers;
using DispatcherWeb.Offices;
using DispatcherWeb.Organizations.Dto;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users
{
    [AbpAuthorize]
    public class UserAppService : DispatcherWebAppServiceBase, IUserAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IRepository<RolePermissionSetting, long> _rolePermissionRepository;
        private readonly IRepository<UserPermissionSetting, long> _userPermissionRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRoleManagementConfig _roleManagementConfig;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Quotes.Quote> _quoteRepository;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly ICustomerContactUserLinkService _customerContactUserLinkService;
        private readonly IUserListCsvExporter _userListCsvExporter;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IUserCreatorService _userCreatorService;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;
        private readonly ILeaseHaulerUserAppService _leaseHaulerUserService;

        public UserAppService(
            RoleManager roleManager,
            IUserEmailer userEmailer,
            IRepository<RolePermissionSetting, long> rolePermissionRepository,
            IRepository<UserPermissionSetting, long> userPermissionRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRoleManagementConfig roleManagementConfig,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Quotes.Quote> quoteRepository,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            IDriverUserLinkService driverUserLinkService,
            ICustomerContactUserLinkService customerContactUserLinkService,
            IUserListCsvExporter userListCsvExporter,
            ISingleOfficeAppService singleOfficeService,
            IUserCreatorService userCreatorService,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer,
            ILeaseHaulerUserAppService leaseHaulerUserService
            )
        {
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
            _userRoleRepository = userRoleRepository;
            _passwordHasher = passwordHasher;
            _organizationUnitRepository = organizationUnitRepository;
            _roleManagementConfig = roleManagementConfig;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
            _roleRepository = roleRepository;
            AppUrlService = NullAppUrlService.Instance;
            _dispatchRepository = dispatchRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _driverRepository = driverRepository;
            _quoteRepository = quoteRepository;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _driverUserLinkService = driverUserLinkService;
            _customerContactUserLinkService = customerContactUserLinkService;
            _userListCsvExporter = userListCsvExporter;
            _singleOfficeService = singleOfficeService;
            _userCreatorService = userCreatorService;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
            _leaseHaulerUserService = leaseHaulerUserService;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
        public async Task<PagedResultDto<UserListDto>> GetUsers(GetUsersInput input)
        {
            var query = await GetUsersFilteredQueryAsync(input);

            var userCount = await query.CountAsync();

            var users = ToUserListDto(query)
                .OrderBy(input.Sorting)
                .PageBy(input);

            var userListDtos = await GetUserListDtoList(users);
            await FillRoleNames(userListDtos);

            return new PagedResultDto<UserListDto>(
                userCount,
                userListDtos
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit)]
        public async Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input)
        {
            var organizationUnitMemberCounts = await (await _userOrganizationUnitRepository.GetQueryAsync())
                .GroupBy(x => x.OrganizationUnitId)
                .Select(groupedUsers => new
                {
                    OrganizationUnitId = groupedUsers.Key,
                    Count = groupedUsers.Count(),
                }).ToDictionaryAsync(x => x.OrganizationUnitId, y => y.Count);

            var organizationUnitRoleCounts = await (await _organizationUnitRoleRepository.GetQueryAsync())
                .GroupBy(x => x.OrganizationUnitId)
                .Select(groupedRoles => new
                {
                    OrganizationUnitId = groupedRoles.Key,
                    Count = groupedRoles.Count(),
                }).ToDictionaryAsync(x => x.OrganizationUnitId, y => y.Count);

            //Getting all available roles
            var userRoleDtos = await (await _roleManager.GetQueryAsync())
                .OrderBy(r => r.DisplayName)
                .Select(r => new UserRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDisplayName = r.DisplayName,
                })
                .ToArrayAsync();

            var allOrganizationUnits = await (await _organizationUnitRepository.GetQueryAsync())
                .Select(x => new OrganizationUnitDto
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Code = x.Code,
                    DisplayName = x.DisplayName,
                    LastModifierUserId = x.LastModifierUserId,
                    LastModificationTime = x.LastModificationTime,
                    MemberCount = organizationUnitMemberCounts.ContainsKey(x.Id) ? organizationUnitMemberCounts[x.Id] : 0,
                    RoleCount = organizationUnitRoleCounts.ContainsKey(x.Id) ? organizationUnitRoleCounts[x.Id] : 0,
                }).ToListAsync();

            var output = new GetUserForEditOutput
            {
                Roles = userRoleDtos,
                AllOrganizationUnits = allOrganizationUnits,
                MemberedOrganizationUnits = new List<long>(),
            };

            if (!input.Id.HasValue)
            {
                //Creating a new user
                output.User = new UserEditDto
                {
                    IsActive = true,
                    ShouldChangePasswordOnNextLogin = true,
                    IsTwoFactorEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                            .TwoFactorLogin.IsEnabled),
                    IsLockoutEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut
                            .IsEnabled),
                };

                foreach (var defaultRole in await (await _roleManager.GetQueryAsync()).Where(r => r.IsDefault).ToListAsync())
                {
                    var defaultUserRole = userRoleDtos.FirstOrDefault(ur => ur.RoleName == defaultRole.Name);
                    if (defaultUserRole != null)
                    {
                        defaultUserRole.IsAssigned = true;
                    }
                }
            }
            else
            {
                //Editing an existing user
                output.User = await (await UserManager.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new UserEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Surname = x.Surname,
                        EmailAddress = x.EmailAddress,
                        PhoneNumber = x.PhoneNumber,
                        IsActive = x.IsActive,
                        IsLockoutEnabled = x.IsLockoutEnabled,
                        ShouldChangePasswordOnNextLogin = x.ShouldChangePasswordOnNextLogin,
                        UserName = x.UserName,
                        OfficeId = x.OfficeId,
                        OfficeName = x.Office.Name,
                        IsTwoFactorEnabled = x.IsTwoFactorEnabled,
                        CustomerContactId = x.CustomerContactId,
                        ProfilePictureId = x.ProfilePictureId,
                        LeaseHaulerId = x.LeaseHaulerUser.LeaseHaulerId,
                        AssignedRoleIds = x.Roles.Select(r => r.RoleId).ToList(),
                        AssignedOrganizationUnitIds = x.OrganizationUnits.Select(ou => ou.OrganizationUnitId).ToList(),
                    })
                    .FirstOrDefaultAsync();

                output.User.IsSingleOffice = await _singleOfficeService.IsSingleOffice();

                output.MemberedOrganizationUnits = output.User.AssignedOrganizationUnitIds;

                var allRolesOfUsersOrganizationUnits = await GetAllRoleNamesOfUsersOrganizationUnitsAsync(input.Id.Value);

                foreach (var userRoleDto in userRoleDtos)
                {
                    userRoleDto.IsAssigned = output.User.AssignedRoleIds.Contains(userRoleDto.RoleId);
                    userRoleDto.InheritedFromOrganizationUnit = allRolesOfUsersOrganizationUnits.Contains(userRoleDto.RoleName);
                }
            }

            var dispatchViaDriverApp = await SettingManager.DispatchViaDriverApplication();
            var allowLeaseHaulers = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit);
            output.Roles = output.Roles.Where(x =>
            {
                switch (x.RoleName)
                {
                    case StaticRoleNames.Tenants.Driver:
                        return x.IsAssigned || dispatchViaDriverApp;
                    case StaticRoleNames.Tenants.LeaseHaulerDriver:
                        return x.IsAssigned || dispatchViaDriverApp && allowLeaseHaulers;
                    default:
                        return true;
                }
            }).ToArray();

            await _singleOfficeService.FillSingleOffice(output.User);

            return output;
        }

        private async Task<List<string>> GetAllRoleNamesOfUsersOrganizationUnitsAsync(long userId)
        {
            var result = await (
                from userOu in await _userOrganizationUnitRepository.GetQueryAsync()
                join roleOu in await _organizationUnitRoleRepository.GetQueryAsync() on userOu.OrganizationUnitId equals roleOu.OrganizationUnitId
                join userOuRoles in await _roleRepository.GetQueryAsync() on roleOu.RoleId equals userOuRoles.Id
                where userOu.UserId == userId
                select userOuRoles.Name
            ).ToListAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task<GetUserPermissionsForEditOutput> GetUserPermissionsForEdit(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var permissions = (await PermissionManager.GetAllPermissionsAsync())
                .Select(x => new FlatPermissionDto
                {
                    ParentName = x.Parent?.Name,
                    Name = x.Name,
                    DisplayName = L(x.DisplayName),
                })
                .OrderBy(p => p.DisplayName)
                .ToList();
            var grantedPermissions = await UserManager.GetGrantedPermissionsAsync(user);

            return new GetUserPermissionsForEditOutput
            {
                UserName = user.UserName,
                Permissions = permissions,
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList(),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task ResetUserSpecificPermissions(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            await UserManager.ResetAllPermissionsAsync(user);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task UpdateUserPermissions(UpdateUserPermissionsInput input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var grantedPermissions = PermissionManager.GetPermissionsFromNamesByValidating(input.GrantedPermissionNames);
            await UserManager.SetGrantedPermissionsAsync(user, grantedPermissions);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
        public async Task CreateOrUpdateUser(CreateOrUpdateUserInput input)
        {
            if (input.User.Id.HasValue)
            {
                await UpdateUserAsync(input);
            }
            else
            {
                await CreateUserAsync(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Delete)]
        public async Task DeleteUser(EntityDto<long> input)
        {
            if (input.Id == AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("YouCanNotDeleteOwnAccount"));
            }

            var user = await UserManager.GetUserByIdAsync(input.Id);
            if (await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver))
            {
                if (await HasLeaseHaulerRequests(user.Id))
                {
                    throw new UserFriendlyException(L("UnableToDeleteLhDriverWithRequests"));
                }
                var drivers = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.UserId == user.Id)
                    .ToListAsync();
                drivers.ForEach(x => x.UserId = null);
            }

            if (await (await _quoteRepository.GetQueryAsync()).AnyAsync(x => x.SalesPersonId == input.Id))
            {
                throw new UserFriendlyException(L("UnableToDeleteUserWithAssociatedData"));
            }

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                if (await (await _friendshipRepository.GetQueryAsync()).AnyAsync(x => x.UserId == input.Id || x.FriendUserId == input.Id)
                    || await (await _chatMessageRepository.GetQueryAsync()).AnyAsync(x => x.UserId == input.Id || x.TargetUserId == input.Id))
                {
                    throw new UserFriendlyException(L("UnableToDeleteUserWithAssociatedData"));
                }
            }

            await _driverUserLinkService.EnsureCanDeleteUser(user);
            await _customerContactUserLinkService.EnsureCanDeleteUser(user);
            CheckErrors(await UserManager.DeleteAsync(user));
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Unlock)]
        public async Task UnlockUser(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            user.Unlock();
            await UserManager.UpdateAsync(user);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Edit)]
        protected virtual async Task UpdateUserAsync(CreateOrUpdateUserInput input)
        {
            Debug.Assert(input.User.Id != null, "input.User.Id should be set.");

            var user = await UserManager.FindByIdAsync(input.User.Id.Value.ToString());
            if (user.IsActive && !input.User.IsActive)
            {
                if (await HasOpenDispatchesAsync(user.Id))
                {
                    throw new UserFriendlyException(L("CantDeactivateUserBecauseOfDispatches"));
                }
            }
            var officeHasChanged = user.OfficeId != input.User.OfficeId;

            //Update user properties
            user.Name = input.User.Name;
            user.Surname = input.User.Surname;
            user.UserName = input.User.UserName;
            user.EmailAddress = input.User.EmailAddress;
            user.PhoneNumber = input.User.PhoneNumber;
            user.Office = null;
            user.OfficeId = input.User.OfficeId;
            user.IsActive = input.User.IsActive;
            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;
            user.IsTwoFactorEnabled = input.User.IsTwoFactorEnabled;
            user.IsLockoutEnabled = input.User.IsLockoutEnabled;

            CheckErrors(await UserManager.UpdateAsync(user));

            if (input.SetRandomPassword)
            {
                var randomPassword = await UserManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.User.Password));
            }

            CheckErrors(await UserManager.UpdateAsync(user));

            //Update roles
            var currentUser = await UserManager.GetUserByIdAsync(AbpSession.GetUserId());
            if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Tenants.Admin))
            {
                //if current user is not Admin, do not allow to add or remove Admin role
                var hasAdminRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Admin);
                if (hasAdminRole && !input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Admin))
                {
                    input.AssignedRoleNames = input.AssignedRoleNames.Union(new[] { StaticRoleNames.Tenants.Admin }).ToArray();
                }
                else if (!hasAdminRole && input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Admin))
                {
                    input.AssignedRoleNames = input.AssignedRoleNames.Except(new[] { StaticRoleNames.Tenants.Admin }).ToArray();
                }
            }
            if (await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Driver)
                && !input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Driver))
            {
                if (await HasOpenDispatchesAsync(user.Id))
                {
                    throw new UserFriendlyException(L("CantRemoveDriverRoleBecauseOfDispatches"));
                }
            }
            if (input.AssignedRoleNames.Contains(StaticRoleNames.Tenants.Customer)
                && !await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Customer)
                && user.CustomerContactId == null)
            {
                throw new UserFriendlyException(L("AssigningCustomerRoleManuallyIsNotSupported"));
            }
            CheckErrors(await UserManager.SetRolesAsync(user, input.AssignedRoleNames));

            if ((await AbpSession.GetTenantIdOrNullAsync()).HasValue)
            {
                await _driverUserLinkService.UpdateDriver(user);
                await _customerContactUserLinkService.UpdateCustomerContact(user);
                await _leaseHaulerUserService.UpdateLeaseHaulerUser(input.LeaseHaulerId, user.Id, user.TenantId);
            }

            //update organization units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            if (officeHasChanged && user.OfficeId.HasValue)
            {
                await _officeOrganizationUnitSynchronizer.AddUserToOrganizationUnitForOfficeId(user.Id, user.OfficeId.Value);
            }

            if (input.SendActivationEmail)
            {
                var emailTemplate = await _userCreatorService.GetActivationEmailTemplate(input.AssignedRoleNames);

                user.SetNewEmailConfirmationCode();
                await UserManager.UpdateAsync(user);

                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    await AppUrlService.CreateEmailActivationUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync()),
                    input.User.Password,
                    emailTemplate.SubjectTemplate,
                    emailTemplate.BodyTemplate
                );
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create)]
        protected virtual async Task CreateUserAsync(CreateOrUpdateUserInput input)
        {
            var user = await _userCreatorService.CreateUser(input);

            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (tenantId.HasValue)
            {
                await _driverUserLinkService.UpdateDriver(user);
                await _leaseHaulerUserService.UpdateLeaseHaulerUser(input.LeaseHaulerId, user.Id, user.TenantId);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task FillRoleNames(IReadOnlyCollection<UserListDto> users)
        {
            if (!users.Any(u => u.Roles.Any()))
            {
                return;
            }

            var roleNames = await (await _roleManager.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.DisplayName,
                }).ToDictionaryAsync(x => x.Id, x => x.DisplayName);

            foreach (var user in users)
            {
                foreach (var role in user.Roles)
                {
                    role.RoleName = roleNames.TryGetValue(role.RoleId, out var roleName) ? roleName : null;
                }

                user.Roles = user.Roles.Where(r => r.RoleName != null).OrderBy(r => r.RoleName).ToList();
            }
        }

        private async Task<IQueryable<User>> GetUsersFilteredQueryAsync(IGetUsersInput input)
        {
            var query = (await UserManager.GetQueryAsync())
                .Include(u => u.Office)
                .WhereIf(input.Role.HasValue, u => u.Roles.Any(r => r.RoleId == input.Role.Value))
                .WhereIf(input.OnlyLockedUsers,
                    u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTime.UtcNow)
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    u =>
                        u.Name.Contains(input.Filter)
                        || u.Surname.Contains(input.Filter)
                        || u.UserName.Contains(input.Filter)
                        || u.EmailAddress.Contains(input.Filter)
                )
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId);

            if (input.Permissions != null && input.Permissions.Any(p => !p.IsNullOrWhiteSpace()))
            {
                var multiTenancySide = await AbpSession.GetMultiTenancySideAsync();
                var staticRoleNames = _roleManagementConfig.StaticRoles.Where(
                    r => r.GrantAllPermissionsByDefault
                         && r.Side == multiTenancySide
                ).Select(r => r.RoleName).ToList();

                input.Permissions = input.Permissions.Where(p => !string.IsNullOrEmpty(p)).ToList();

                var userIds = from user in query
                              join ur in await _userRoleRepository.GetQueryAsync() on user.Id equals ur.UserId into urJoined
                              from ur in urJoined.DefaultIfEmpty()
                              join urr in await _roleRepository.GetQueryAsync() on ur.RoleId equals urr.Id into urrJoined
                              from urr in urrJoined.DefaultIfEmpty()
                              join up in (await _userPermissionRepository.GetQueryAsync())
                                  .Where(userPermission => input.Permissions.Contains(userPermission.Name)) on user.Id equals up.UserId into upJoined
                              from up in upJoined.DefaultIfEmpty()
                              join rp in (await _rolePermissionRepository.GetQueryAsync())
                                  .Where(rolePermission => input.Permissions.Contains(rolePermission.Name)) on
                                  new { RoleId = ur == null ? 0 : ur.RoleId } equals new { rp.RoleId } into rpJoined
                              from rp in rpJoined.DefaultIfEmpty()
                              where (up != null && up.IsGranted)
                                    || (up == null && rp != null && rp.IsGranted)
                                    || (up == null && rp == null && staticRoleNames.Contains(urr.Name))
                              group user by user.Id
                    into userGrouped
                              select userGrouped.Key;

                query = (await UserManager.GetQueryAsync()).Where(e => userIds.Contains(e.Id));
            }

            return query;
        }

        private IQueryable<UserListDto> ToUserListDto(IQueryable<User> users)
        {
            return users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    UserName = u.UserName,
                    EmailAddress = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    ProfilePictureId = u.ProfilePictureId,
                    IsEmailConfirmed = u.IsEmailConfirmed,
                    OfficeName = u.Office.Name,
                    LastLoginTime = u.LastLoginTime,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLockoutEnabled,
                    CreationTime = u.CreationTime,
                    Roles = u.Roles.Select(r => new UserListRoleDto
                    {
                        RoleId = r.RoleId,
                    }).ToList(),
                });
        }

        private async Task<List<UserListDto>> GetUserListDtoList(IQueryable<UserListDto> users)
        {
            var userListDtos = await users.ToListAsync();

            await FillRoleNames(userListDtos);
            return userListDtos;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
        public async Task<FileDto> GetUsersToCsv(GetUsersInput input)
        {
            var query = await GetUsersFilteredQueryAsync(input);
            var users = ToUserListDto(query)
                .OrderBy(input.Sorting);

            var items = await GetUserListDtoList(users);

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return await _userListCsvExporter.ExportToFileAsync(items);
        }

        private async Task<bool> HasOpenDispatchesAsync(long userId)
        {
            return await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.Driver.UserId == userId && !Dispatch.ClosedDispatchStatuses.Contains(x.Status));
        }

        private async Task<bool> HasLeaseHaulerRequests(long userId)
        {
            return await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .AnyAsync(x => x.Driver.UserId == userId);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Users)]
        public async Task<PagedResultDto<SelectListDto>> GetMaintenanceUsersSelectList(GetSelectListInput input)
        {
            int[] maintenanceRolesIdArray = await (await _roleManager.GetAvailableRolesAsync())
                .Where(r => r.Name == StaticRoleNames.Tenants.Maintenance || r.Name == StaticRoleNames.Tenants.MaintenanceSupervisor)
                .Select(r => r.Id)
                .ToArrayAsync();

            return await (await UserManager.GetQueryAsync())
                    .Where(u => u.Roles.Any(r => maintenanceRolesIdArray.Contains(r.RoleId)))
                    .Select(u => new SelectListDto
                    {
                        Id = u.Id.ToString(),
                        Name = u.Name + " " + u.Surname,
                    })
                    .GetSelectListResult(input)
                ;

        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Users)]
        public async Task<PagedResultDto<SelectListDto>> GetUsersSelectList(GetSelectListInput input)
        {
            var query = (await UserManager.GetQueryAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Users)]
        public async Task<PagedResultDto<SelectListDto>> GetSalespersonsSelectList(GetSelectListInput input)
        {
            var query = await UserManager.GetUsersWithGrantedPermission(_roleManager, AppPermissions.CanBeSalesperson);

            return await query
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname,
                })
                .GetSelectListResult(input);
        }

    }
}
