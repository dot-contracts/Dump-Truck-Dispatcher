using Abp.Authorization;
using Abp.UI;
using DispatcherWeb.Authorization.Accounts.Dto;
using DispatcherWeb.DriverApp.Account.Dto;
using ResetPasswordInput = DispatcherWeb.DriverApp.Account.Dto.ResetPasswordInput;
using SendPasswordResetCodeInput = DispatcherWeb.DriverApp.Account.Dto.SendPasswordResetCodeInput;

namespace DispatcherWeb.DriverApp.Account
{
    [AbpAuthorize]
    public class AccountAppService : DispatcherWebDriverAppAppServiceBase, IAccountAppService
    {
        private readonly Authorization.Accounts.IAccountAppService _accountAppService;

        public AccountAppService(
            Authorization.Accounts.IAccountAppService accountAppService
            )
        {
            _accountAppService = accountAppService;
        }

        [AbpAllowAnonymous]
        public async Task SendPasswordResetCode(SendPasswordResetCodeInput input)
        {
            var tenant = await SwitchToTenantIfNeeded(input.TenancyName);

            using (AbpSession.Use(tenant.TenantId, AbpSession.UserId))
            {
                await _accountAppService.SendPasswordResetCode(input);
            }
        }

        [AbpAllowAnonymous]
        public async Task<ResetPasswordOutput> ResetPassword(ResetPasswordInput input)
        {
            var tenant = await SwitchToTenantIfNeeded(input.TenancyName);
            var user = await UserManager.FindByEmailAsync(input.EmailAddress);
            if (user == null)
            {
                throw new UserFriendlyException("User with the specified email address wasn't found");
            }

            using (AbpSession.Use(tenant.TenantId, AbpSession.UserId))
            {
                return await _accountAppService.ResetPassword(new Authorization.Accounts.Dto.ResetPasswordInput
                {
                    UserId = user.Id,
                    Password = input.Password,
                    ResetCode = input.ResetCode,
                });
            }
        }

        [AbpAllowAnonymous]
        public async Task ValidateTenant(ValidateTenantInput input)
        {
            await SwitchToTenantIfNeeded(input.TenancyName);
        }

        [AbpAllowAnonymous]
        public Task TestUserFriendlyException()
        {
            throw new UserFriendlyException("test error message", "test error details");
        }

        [AbpAllowAnonymous]
        public Task TestInternalError()
        {
            throw new ApplicationException("application exception test");
        }

        private async Task<IsTenantAvailableOutput> SwitchToTenantIfNeeded(string tenancyName)
        {
            var tenantResult = await _accountAppService.IsTenantAvailable(new IsTenantAvailableInput
            {
                TenancyName = tenancyName,
            });

            switch (tenantResult.State)
            {
                case TenantAvailabilityState.Available: break;
                case TenantAvailabilityState.InActive: throw new UserFriendlyException(L("TenantIsNotActive", tenancyName));
                case TenantAvailabilityState.NotFound: throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
            }

            if (tenantResult.TenantId != await AbpSession.GetTenantIdOrNullAsync())
            {
                CurrentUnitOfWork.SetTenantId(tenantResult.TenantId);
                AbpSession.Use(tenantResult.TenantId, AbpSession.UserId);
            }

            return tenantResult;
        }
    }
}
