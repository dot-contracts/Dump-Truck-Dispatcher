using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users
{
    [AbpAuthorize]
    public class UserLinkAppService : DispatcherWebAppServiceBase, IUserLinkAppService
    {
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly IUserLinkManager _userLinkManager;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<UserAccount, long> _userAccountRepository;
        private readonly LogInManager _logInManager;

        public UserLinkAppService(
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            IUserLinkManager userLinkManager,
            IRepository<Tenant> tenantRepository,
            IRepository<UserAccount, long> userAccountRepository,
            LogInManager logInManager)
        {
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _userLinkManager = userLinkManager;
            _tenantRepository = tenantRepository;
            _userAccountRepository = userAccountRepository;
            _logInManager = logInManager;
        }

        public async Task LinkToUser(LinkToUserInput input)
        {
            var loginResult = await _logInManager.LoginAsync(input.UsernameOrEmailAddress, input.Password, input.TenancyName);

            if (loginResult.Result != AbpLoginResultType.Success)
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, input.UsernameOrEmailAddress, input.TenancyName);
            }

            if (await AbpSession.IsUserAsync(loginResult.User))
            {
                throw new UserFriendlyException(L("YouCannotLinkToSameAccount"));
            }

            if (loginResult.User.ShouldChangePasswordOnNextLogin)
            {
                throw new UserFriendlyException(L("ChangePasswordBeforeLinkToAnAccount"));
            }

            var currentUser = await GetCurrentUserAsync();
            await _userLinkManager.Link(currentUser, loginResult.User);
        }

        public async Task<PagedResultDto<LinkedUserDto>> GetLinkedUsers(GetLinkedUsersInput input)
        {
            var userLinkId = await _userLinkManager.GetUserLinkId(await AbpSession.ToUserIdentifierAsync());
            if (userLinkId == null)
            {
                return new PagedResultDto<LinkedUserDto>(0, new List<LinkedUserDto>());
            }

            var query = (await CreateLinkedUsersQueryAsync(userLinkId.Value))
                .OrderBy(input.Sorting);

            var totalCount = await query.CountAsync();

            var linkedUsers = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<LinkedUserDto>(
                totalCount,
                linkedUsers
            );
        }

        [DisableAuditing]
        public async Task<ListResultDto<LinkedUserDto>> GetRecentlyUsedLinkedUsers()
        {
            var userLinkId = await _userLinkManager.GetUserLinkId(await AbpSession.ToUserIdentifierAsync());
            if (userLinkId == null)
            {
                return new ListResultDto<LinkedUserDto>();
            }

            var query = (await CreateLinkedUsersQueryAsync(userLinkId.Value))
                .OrderBy(x => x.TenancyName)
                .ThenBy(x => x.Username);
            var recentlyUsedLinkedUsers = await query.Take(3).ToListAsync();

            return new ListResultDto<LinkedUserDto>(recentlyUsedLinkedUsers);
        }

        public async Task UnlinkUser(UnlinkUserInput input)
        {
            var userLinkId = await _userLinkManager.GetUserLinkId(await AbpSession.ToUserIdentifierAsync());

            if (!userLinkId.HasValue)
            {
                throw new Exception(L("YouAreNotLinkedToAnyAccount"));
            }

            if (!await _userLinkManager.AreUsersLinked(await AbpSession.ToUserIdentifierAsync(), input.ToUserIdentifier()))
            {
                return;
            }

            await _userLinkManager.Unlink(input.ToUserIdentifier());
        }

        private async Task<IQueryable<LinkedUserDto>> CreateLinkedUsersQueryAsync(long userLinkId)
        {
            var currentUserIdentifier = await AbpSession.ToUserIdentifierAsync();

            return (from userAccount in await _userAccountRepository.GetQueryAsync()
                    join tenant in await _tenantRepository.GetQueryAsync() on userAccount.TenantId equals tenant.Id into tenantJoined
                    from tenant in tenantJoined.DefaultIfEmpty()
                    where
                        (userAccount.TenantId != currentUserIdentifier.TenantId
                        || userAccount.UserId != currentUserIdentifier.UserId)
                        && userAccount.UserLinkId.HasValue
                        && userAccount.UserLinkId == userLinkId
                    select new LinkedUserDto
                    {
                        Id = userAccount.UserId,
                        TenantId = userAccount.TenantId,
                        TenancyName = tenant == null ? "." : tenant.TenancyName,
                        Username = userAccount.UserName,
                    });
        }
    }
}
