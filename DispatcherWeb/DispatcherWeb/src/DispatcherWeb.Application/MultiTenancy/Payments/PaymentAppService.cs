using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Editions;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.MultiTenancy.Payments.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy.Payments
{
    [AbpAuthorize]
    public class PaymentAppService : DispatcherWebAppServiceBase, IPaymentAppService
    {
        private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
        private readonly EditionManager _editionManager;
        private readonly IPaymentGatewayStore _paymentGatewayStore;
        private readonly TenantManager _tenantManager;


        public PaymentAppService(
            ISubscriptionPaymentRepository subscriptionPaymentRepository,
            EditionManager editionManager,
            IPaymentGatewayStore paymentGatewayStore,
            TenantManager tenantManager)
        {
            _subscriptionPaymentRepository = subscriptionPaymentRepository;
            _editionManager = editionManager;
            _paymentGatewayStore = paymentGatewayStore;
            _tenantManager = tenantManager;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)]
        public async Task<PaymentInfoDto> GetPaymentInfo(PaymentInfoInput input)
        {
            var tenant = await TenantManager.GetByIdAsync(await AbpSession.GetTenantIdAsync());

            if (tenant.EditionId == null)
            {
                throw new UserFriendlyException(L("TenantEditionIsNotAssigned"));
            }

            var currentEdition = (SubscribableEdition)await _editionManager.GetByIdAsync(tenant.EditionId.Value);
            var targetEdition = input.UpgradeEditionId == null ? currentEdition : (SubscribableEdition)await _editionManager.GetByIdAsync(input.UpgradeEditionId.Value);

            decimal additionalPrice = 0;

            if (input.UpgradeEditionId.HasValue)
            {
                var lastPayment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(
                    tenantId: await AbpSession.GetTenantIdAsync(),
                    gateway: null,
                    isRecurring: null))
                    .Select(x => new
                    {
                        x.PaymentPeriodType,
                    }).FirstAsync();

                using (UnitOfWorkManager.Current.SetTenantId(null))
                {
                    additionalPrice = await CalculateAmountForPaymentAsync(targetEdition, lastPayment.PaymentPeriodType, EditionPaymentType.Upgrade, tenant);
                }
            }

            var edition = input.UpgradeEditionId == null
                ? new EditionSelectDto
                {
                    Id = currentEdition.Id,
                    Name = currentEdition.Name,
                    DisplayName = currentEdition.DisplayName,
                    DailyPrice = currentEdition.DailyPrice,
                    WeeklyPrice = currentEdition.WeeklyPrice,
                    MonthlyPrice = currentEdition.MonthlyPrice,
                    AnnualPrice = currentEdition.AnnualPrice,
                    TrialDayCount = currentEdition.TrialDayCount,
                    WaitingDayAfterExpire = currentEdition.WaitingDayAfterExpire,
                    IsFree = currentEdition.IsFree,
                }
                : new EditionSelectDto
                {
                    Id = targetEdition.Id,
                    Name = targetEdition.Name,
                    DisplayName = targetEdition.DisplayName,
                    DailyPrice = targetEdition.DailyPrice,
                    WeeklyPrice = targetEdition.WeeklyPrice,
                    MonthlyPrice = targetEdition.MonthlyPrice,
                    AnnualPrice = targetEdition.AnnualPrice,
                    TrialDayCount = targetEdition.TrialDayCount,
                    WaitingDayAfterExpire = targetEdition.WaitingDayAfterExpire,
                    IsFree = targetEdition.IsFree,
                };

            return new PaymentInfoDto
            {
                Edition = edition,
                AdditionalPrice = additionalPrice,
            };
        }

        [AbpAllowAnonymous]
        public async Task<long> CreatePayment(CreatePaymentDto input)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (!tenantId.HasValue)
            {
                throw new ApplicationException("A payment only can be created for a tenant. TenantId is not set in the IAbpSession!");
            }

            decimal amount;
            string targetEditionName;

            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                var targetEdition = (SubscribableEdition)await _editionManager.GetByIdAsync(input.EditionId);
                targetEditionName = targetEdition.DisplayName;

                var tenant = await TenantManager.GetByIdAsync(tenantId.Value);
                amount = await CalculateAmountForPaymentAsync(targetEdition, input.PaymentPeriodType, input.EditionPaymentType, tenant);
            }

            var payment = new SubscriptionPayment
            {
                Description = GetPaymentDescription(input.EditionPaymentType, input.PaymentPeriodType, targetEditionName, input.RecurringPaymentEnabled),
                PaymentPeriodType = input.PaymentPeriodType,
                EditionId = input.EditionId,
                TenantId = tenantId.Value,
                Gateway = input.SubscriptionPaymentGatewayType,
                Amount = amount,
                DayCount = input.PaymentPeriodType.HasValue ? (int)input.PaymentPeriodType.Value : 0,
                IsRecurring = input.RecurringPaymentEnabled,
                SuccessUrl = input.SuccessUrl,
                ErrorUrl = input.ErrorUrl,
                EditionPaymentType = input.EditionPaymentType,
            };

            return await _subscriptionPaymentRepository.InsertAndGetIdAsync(payment);
        }

        [AbpAllowAnonymous]
        public async Task CancelPayment(CancelPaymentDto input)
        {
            var payment = await _subscriptionPaymentRepository.GetByGatewayAndPaymentIdAsync(
                    input.Gateway,
                    input.PaymentId
                );

            payment.SetAsCancelled();
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<SubscriptionPaymentListDto>> GetPaymentHistory(GetPaymentHistoryInput input)
        {
            var tenantId = await AbpSession.GetTenantIdAsync();
            var query = (await _subscriptionPaymentRepository.GetQueryAsync())
                .Include(sp => sp.Edition)
                .Where(sp => sp.TenantId == tenantId)
                .OrderBy(input.Sorting);

            var payments = await query
                .Select(x => new SubscriptionPaymentListDto
                {
                    Id = x.Id,
                    Gateway = x.Gateway.ToString(),
                    Amount = x.Amount,
                    EditionId = x.EditionId,
                    PaymentPeriodType = x.PaymentPeriodType.ToString(),
                    EditionDisplayName = x.Edition.DisplayName,
                    PaymentId = x.ExternalPaymentId,
                    Status = x.Status.ToString(),
                    TenantId = x.TenantId,
                    InvoiceNo = x.InvoiceNo,
                    CreationTime = x.CreationTime,
                }).OrderBy(input.Sorting).PageBy(input).ToListAsync();

            var paymentsCount = query.Count();

            return new PagedResultDto<SubscriptionPaymentListDto>(paymentsCount, payments);
        }

        [AbpAllowAnonymous]
        public List<PaymentGatewayModel> GetActiveGateways(GetActiveGatewaysInput input)
        {
            return _paymentGatewayStore.GetActiveGateways()
                .WhereIf(input.RecurringPaymentsEnabled.HasValue, gateway => gateway.SupportsRecurringPayments == input.RecurringPaymentsEnabled.Value)
                .ToList();
        }

        [AbpAllowAnonymous]
        public async Task<SubscriptionPaymentDto> GetPaymentAsync(long paymentId)
        {
            return await (await _subscriptionPaymentRepository.GetQueryAsync())
                .Where(x => x.Id == paymentId)
                .Select(x => new SubscriptionPaymentDto
                {
                    Amount = x.Amount,
                    EditionId = x.EditionId,
                    PaymentId = x.Id.ToString(),
                    PaymentPeriodType = x.PaymentPeriodType,
                    DayCount = x.DayCount,
                    TenantId = x.TenantId,
                }).FirstOrDefaultAsync();
        }

        [AbpAllowAnonymous]
        public async Task<SubscriptionPaymentDto> GetLastCompletedPayment()
        {
            var payment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(
                tenantId: await AbpSession.GetTenantIdAsync(),
                gateway: null,
                isRecurring: null))
                .Select(x => new SubscriptionPaymentDto
                {
                    Gateway = x.Gateway,
                    Amount = x.Amount,
                    EditionId = x.EditionId,
                    TenantId = x.TenantId,
                    DayCount = x.DayCount,
                    PaymentPeriodType = x.PaymentPeriodType,
                    EditionDisplayName = x.Edition.DisplayName,
                    InvoiceNo = x.InvoiceNo,
                })
                .FirstAsync();

            return payment;
        }

        [AbpAllowAnonymous]
        public async Task BuyNowSucceed(long paymentId)
        {
            var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);

            if (payment.Status != SubscriptionPaymentStatus.Paid)
            {
                throw new ApplicationException("Your payment is not completed !");
            }

            payment.SetAsCompleted();

            await _tenantManager.UpdateTenantAsync(
                payment.TenantId,
                true,
                false,
                payment.PaymentPeriodType,
                payment.EditionId,
                EditionPaymentType.BuyNow
            );
        }

        [AbpAllowAnonymous]
        public async Task NewRegistrationSucceed(long paymentId)
        {
            var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);
            if (payment.Status != SubscriptionPaymentStatus.Paid)
            {
                throw new ApplicationException("Your payment is not completed !");
            }

            payment.SetAsCompleted();

            await _tenantManager.UpdateTenantAsync(
                payment.TenantId,
                true,
                null,
                payment.PaymentPeriodType,
                payment.EditionId,
                EditionPaymentType.NewRegistration
            );
        }

        [AbpAllowAnonymous]
        public async Task UpgradeSucceed(long paymentId)
        {
            var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);
            if (payment.Status != SubscriptionPaymentStatus.Paid)
            {
                throw new ApplicationException("Your payment is not completed !");
            }

            payment.SetAsCompleted();

            await _tenantManager.UpdateTenantAsync(
                payment.TenantId,
                true,
                null,
                payment.PaymentPeriodType,
                payment.EditionId,
                EditionPaymentType.Upgrade
            );
        }

        [AbpAllowAnonymous]
        public async Task ExtendSucceed(long paymentId)
        {
            var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);
            if (payment.Status != SubscriptionPaymentStatus.Paid)
            {
                throw new ApplicationException("Your payment is not completed !");
            }

            payment.SetAsCompleted();

            await _tenantManager.UpdateTenantAsync(
                payment.TenantId,
                true,
                null,
                payment.PaymentPeriodType,
                payment.EditionId,
                EditionPaymentType.Extend
            );
        }

        [AbpAllowAnonymous]
        public async Task PaymentFailed(long paymentId)
        {
            var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);
            payment.SetAsFailed();
        }

        private async Task<decimal> CalculateAmountForPaymentAsync(SubscribableEdition targetEdition, PaymentPeriodType? periodType, EditionPaymentType editionPaymentType, Tenant tenant)
        {
            if (editionPaymentType != EditionPaymentType.Upgrade)
            {
                return targetEdition.GetPaymentAmount(periodType);
            }

            if (tenant.EditionId == null)
            {
                throw new UserFriendlyException(L("CanNotUpgradeSubscriptionSinceTenantHasNoEditionAssigned"));
            }

            var remainingHoursCount = tenant.CalculateRemainingHoursCount();

            if (remainingHoursCount <= 0)
            {
                return targetEdition.GetPaymentAmount(periodType);
            }

            Debug.Assert(tenant.EditionId != null, "tenant.EditionId != null");

            var currentEdition = (SubscribableEdition)await _editionManager.GetByIdAsync(tenant.EditionId.Value);

            var lastPayment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(tenant.Id, null, null))
                .Select(x => new
                {
                    x.PaymentPeriodType,
                }).FirstOrDefaultAsync();

            if (lastPayment?.PaymentPeriodType == null)
            {
                throw new ApplicationException("There is no completed payment record !");
            }

            return TenantManager.GetUpgradePrice(currentEdition, targetEdition, remainingHoursCount, lastPayment.PaymentPeriodType.Value);
        }

        private string GetPaymentDescription(EditionPaymentType editionPaymentType, PaymentPeriodType? paymentPeriodType, string targetEditionName, bool isRecurring)
        {
            var description = L(editionPaymentType + "_Edition_Description", targetEditionName);

            if (!paymentPeriodType.HasValue)
            {
                if (isRecurring && editionPaymentType == EditionPaymentType.Upgrade)
                {
                    description += " (" + L("CostOfProration") + ")";
                }

                return description;
            }

            if (editionPaymentType == EditionPaymentType.NewRegistration || editionPaymentType == EditionPaymentType.BuyNow)
            {
                description += " (" + L(paymentPeriodType.Value.ToString()) + ")";
            }

            if (isRecurring && editionPaymentType == EditionPaymentType.Upgrade)
            {
                description += " (" + L("CostOfProration") + ")";
            }

            return description;
        }

        [AbpAllowAnonymous]
        public async Task SwitchBetweenFreeEditions(int upgradeEditionId)
        {
            var tenant = await _tenantManager.GetByIdAsync(await AbpSession.GetTenantIdAsync());

            if (!tenant.EditionId.HasValue)
            {
                throw new ArgumentException("tenant.EditionId can not be null");
            }

            var currentEdition = await _editionManager.GetByIdAsync(tenant.EditionId.Value);
            if (!((SubscribableEdition)currentEdition).IsFree)
            {
                throw new ArgumentException("You can only switch between free editions. Current edition if not free");
            }

            var upgradeEdition = await _editionManager.GetByIdAsync(upgradeEditionId);
            if (!((SubscribableEdition)upgradeEdition).IsFree)
            {
                throw new ArgumentException("You can only switch between free editions. Target edition if not free");
            }

            await _tenantManager.UpdateTenantAsync(
                    tenant.Id,
                    true,
                    null,
                    null,
                    upgradeEditionId,
                    EditionPaymentType.Upgrade
                );
        }

        [AbpAllowAnonymous]
        public async Task UpgradeSubscriptionCostsLessThenMinAmount(int editionId)
        {
            var paymentInfo = await GetPaymentInfo(new PaymentInfoInput { UpgradeEditionId = editionId });

            if (!paymentInfo.IsLessThanMinimumUpgradePaymentAmount())
            {
                throw new ApplicationException("Subscription payment requires more than minimum upgrade payment amount. Use payment gateway to charge payment amount.");
            }

            var lastPayment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(
                tenantId: await AbpSession.GetTenantIdAsync(),
                gateway: null,
                isRecurring: null))
                .AsNoTracking()
                .FirstAsync();

            await _tenantManager.UpdateTenantAsync(
                await AbpSession.GetTenantIdAsync(),
                true,
                null,
                SubscriptionPayment.GetPaymentPeriodType(lastPayment.DayCount),
                editionId,
                EditionPaymentType.Upgrade
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)]
        public async Task<bool> HasAnyPayment()
        {
            return await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(
                       tenantId: await AbpSession.GetTenantIdAsync(),
                       gateway: null,
                       isRecurring: null))
                .AnyAsync();
        }
    }
}
