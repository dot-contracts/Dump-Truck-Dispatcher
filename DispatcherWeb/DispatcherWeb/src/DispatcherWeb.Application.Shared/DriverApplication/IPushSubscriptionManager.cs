using System.Threading.Tasks;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.DriverApplication
{
    public interface IPushSubscriptionManager
    {
        Task AddDriverPushSubscription(AddDriverPushSubscriptionInput input);
        Task RemoveDriverPushSubscription(RemoveDriverPushSubscriptionInput input);
        Task CleanupSubscriptions();
    }
}
