using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.MultiTenancy.Payments;

namespace DispatcherWeb.MultiTenancy
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)]
    public class SubscriptionAppService : DispatcherWebAppServiceBase, ISubscriptionAppService
    {
        public IEventBus EventBus { get; set; }

        public SubscriptionAppService()
        {
            EventBus = NullEventBus.Instance;
        }

        public async Task DisableRecurringPayments()
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var tenant = await TenantManager.GetByIdAsync(await AbpSession.GetTenantIdAsync());
                if (tenant.SubscriptionPaymentType == SubscriptionPaymentType.RecurringAutomatic)
                {
                    tenant.SubscriptionPaymentType = SubscriptionPaymentType.RecurringManual;
                    await EventBus.TriggerAsync(new RecurringPaymentsDisabledEventData
                    {
                        TenantId = await AbpSession.GetTenantIdAsync(),
                        EditionId = tenant.EditionId.Value,
                    });
                }
            }
        }

        public async Task EnableRecurringPayments()
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var tenant = await TenantManager.GetByIdAsync(await AbpSession.GetTenantIdAsync());
                if (tenant.SubscriptionPaymentType == SubscriptionPaymentType.RecurringManual)
                {
                    tenant.SubscriptionPaymentType = SubscriptionPaymentType.RecurringAutomatic;
                    tenant.SubscriptionEndDateUtc = null;

                    await EventBus.TriggerAsync(new RecurringPaymentsEnabledEventData
                    {
                        TenantId = await AbpSession.GetTenantIdAsync(),
                    });
                }
            }
        }
    }
}
