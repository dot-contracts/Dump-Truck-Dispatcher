using Abp.Events.Bus.Handlers;

namespace DispatcherWeb.MultiTenancy.Payments
{
    public interface ISupportsRecurringPayments :
        IAsyncEventHandler<RecurringPaymentsDisabledEventData>,
        IAsyncEventHandler<RecurringPaymentsEnabledEventData>,
        IAsyncEventHandler<TenantEditionChangedEventData>
    {

    }
}
