using Abp.Events.Bus;

namespace DispatcherWeb.MultiTenancy
{
    public class RecurringPaymentsEnabledEventData : EventData
    {
        public int TenantId { get; set; }
    }
}