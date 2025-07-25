using System.Security.Claims;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Uow;
using DispatcherWeb.Authorization.Cache;
using DispatcherWeb.Authorization.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DispatcherWeb.Authorization.Users
{
    public class UserClaimsPrincipalFactory : AbpUserClaimsPrincipalFactory<User, Role>
    {
        private readonly IUserClaimsCacheHelper _userClaimsCacheHelper;
        public new UserManager UserManager { get; }

        public UserClaimsPrincipalFactory(
            IUserClaimsCacheHelper userClaimsCacheHelper,
            UserManager userManager,
            RoleManager roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IUnitOfWorkManager unitOfWorkManager)
            : base(
                  userManager,
                  roleManager,
                  optionsAccessor,
                  unitOfWorkManager)
        {
            _userClaimsCacheHelper = userClaimsCacheHelper;
            UserManager = userManager;
        }
        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);

            var userClaims = await _userClaimsCacheHelper.GetUserClaimsAsync(user.Id);

            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeId, user.OfficeId + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeName, userClaims.OfficeName ?? ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserOfficeCopyChargeTo, userClaims.OfficeCopyChargeTo == true ? "true" : "false"));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserCustomerId, userClaims.CustomerId + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserCustomerName, userClaims.CustomerName + ""));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserName, user.Name));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserEmail, user.EmailAddress));
                identity.AddClaim(new Claim(DispatcherWebConsts.Claims.UserLeaseHaulerId, userClaims.LeaseHaulerId + ""));
            }

            return principal;
        }
    }
}
