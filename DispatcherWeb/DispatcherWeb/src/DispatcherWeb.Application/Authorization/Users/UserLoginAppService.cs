using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Users.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users
{
    [AbpAuthorize]
    public class UserLoginAppService : DispatcherWebAppServiceBase, IUserLoginAppService
    {
        private readonly IRepository<UserLoginAttempt, long> _userLoginAttemptRepository;

        public UserLoginAppService(IRepository<UserLoginAttempt, long> userLoginAttemptRepository)
        {
            _userLoginAttemptRepository = userLoginAttemptRepository;
        }

        [DisableAuditing]
        public async Task<PagedResultDto<UserLoginAttemptDto>> GetUserLoginAttempts(GetLoginAttemptsInput input)
        {
            var userId = AbpSession.GetUserId();
            var query = (await _userLoginAttemptRepository.GetQueryAsync())
                .Where(la => la.UserId == userId)
                .WhereIf(!input.Filter.IsNullOrEmpty(), la => la.ClientIpAddress.Contains(input.Filter) || la.BrowserInfo.Contains(input.Filter))
                .WhereIf(input.StartDate.HasValue, la => la.CreationTime >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, la => la.CreationTime <= input.EndDate)
                .WhereIf(input.Result.HasValue, la => la.Result == input.Result);

            var loginAttemptCount = await query.CountAsync();

            var loginAttempts = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .Select(x => new UserLoginAttemptDto
                {
                    TenancyName = x.TenancyName,
                    UserNameOrEmail = x.UserNameOrEmailAddress,
                    ClientIpAddress = x.ClientIpAddress,
                    ClientName = x.ClientName,
                    BrowserInfo = x.BrowserInfo,
                    Result = x.Result.ToString(),
                    CreationTime = x.CreationTime,
                })
                .ToListAsync();

            return new PagedResultDto<UserLoginAttemptDto>(
                loginAttemptCount,
                loginAttempts
            );
        }
    }
}
