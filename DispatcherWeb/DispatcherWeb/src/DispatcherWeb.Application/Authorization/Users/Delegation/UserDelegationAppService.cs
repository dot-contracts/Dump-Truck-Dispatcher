using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization.Delegation;
using DispatcherWeb.Authorization.Users.Delegation.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Delegation
{
    [AbpAuthorize]
    public class UserDelegationAppService : DispatcherWebAppServiceBase, IUserDelegationAppService
    {
        private readonly IRepository<UserDelegation, long> _userDelegationRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUserDelegationManager _userDelegationManager;
        private readonly IUserDelegationConfiguration _userDelegationConfiguration;

        public UserDelegationAppService(
            IRepository<UserDelegation, long> userDelegationRepository,
            IRepository<User, long> userRepository,
            IUserDelegationManager userDelegationManager,
            IUserDelegationConfiguration userDelegationConfiguration)
        {
            _userDelegationRepository = userDelegationRepository;
            _userRepository = userRepository;
            _userDelegationManager = userDelegationManager;
            _userDelegationConfiguration = userDelegationConfiguration;
        }

        public async Task<PagedResultDto<UserDelegationDto>> GetDelegatedUsers(GetUserDelegationsInput input)
        {
            CheckUserDelegationOperation();

            var query = await CreateDelegatedUsersQueryAsync(sourceUserId: AbpSession.GetUserId(), targetUserId: null, input.Sorting);
            var totalCount = await query.CountAsync();

            var userDelegations = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<UserDelegationDto>(
                totalCount,
                userDelegations
            );
        }

        public async Task DelegateNewUser(CreateUserDelegationDto input)
        {
            if (input.TargetUserId == AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("SelfUserDelegationErrorMessage"));
            }

            CheckUserDelegationOperation();

            var delegation = new UserDelegation
            {
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                SourceUserId = AbpSession.GetUserId(),
                TargetUserId = input.TargetUserId,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
            };

            await _userDelegationRepository.InsertAsync(delegation);
        }

        public async Task RemoveDelegation(EntityDto<long> input)
        {
            CheckUserDelegationOperation();

            await _userDelegationManager.RemoveDelegationAsync(input.Id, await AbpSession.ToUserIdentifierAsync());
        }

        /// <summary>
        /// Returns active user delegations for current user
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserDelegationDto>> GetActiveUserDelegations()
        {
            var query = await CreateActiveUserDelegationsQueryAsync(AbpSession.GetUserId(), "username");
            query = query.Where(e => e.EndTime >= Clock.Now);
            return await query.ToListAsync();
        }

        private void CheckUserDelegationOperation()
        {
            if (AbpSession.ImpersonatorUserId.HasValue)
            {
                throw new Exception("Cannot execute user delegation operations with an impersonated user !");
            }

            if (!_userDelegationConfiguration.IsEnabled)
            {
                throw new Exception("User delegation configuration is not enabled !");
            }
        }

        private async Task<IQueryable<UserDelegationDto>> CreateDelegatedUsersQueryAsync(long? sourceUserId, long? targetUserId, string sorting)
        {
            var query = (await _userDelegationRepository.GetQueryAsync())
                .WhereIf(sourceUserId.HasValue, e => e.SourceUserId == sourceUserId)
                .WhereIf(targetUserId.HasValue, e => e.TargetUserId == targetUserId);

            return (from userDelegation in query
                    join targetUser in await _userRepository.GetQueryAsync() on userDelegation.TargetUserId equals targetUser.Id into targetUserJoined
                    from targetUser in targetUserJoined.DefaultIfEmpty()
                    select new UserDelegationDto
                    {
                        Id = userDelegation.Id,
                        Username = targetUser.UserName,
                        StartTime = userDelegation.StartTime,
                        EndTime = userDelegation.EndTime,
                    }).OrderBy(sorting);
        }

        private async Task<IQueryable<UserDelegationDto>> CreateActiveUserDelegationsQueryAsync(long targetUserId, string sorting)
        {
            var query = (await _userDelegationRepository.GetQueryAsync())
                .Where(e => e.TargetUserId == targetUserId);

            return (from userDelegation in query
                    join sourceUser in await _userRepository.GetQueryAsync() on userDelegation.SourceUserId equals sourceUser.Id into sourceUserJoined
                    from sourceUser in sourceUserJoined.DefaultIfEmpty()
                    select new UserDelegationDto
                    {
                        Id = userDelegation.Id,
                        Username = sourceUser.UserName,
                        StartTime = userDelegation.StartTime,
                        EndTime = userDelegation.EndTime,
                    }).OrderBy(sorting);
        }
    }
}
