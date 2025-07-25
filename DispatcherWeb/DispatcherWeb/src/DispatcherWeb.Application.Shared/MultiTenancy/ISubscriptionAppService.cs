using System.Threading.Tasks;
using Abp.Application.Services;

namespace DispatcherWeb.MultiTenancy
{
    public interface ISubscriptionAppService : IApplicationService
    {
        Task DisableRecurringPayments();

        Task EnableRecurringPayments();
    }
}
