using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulerRequests.Dto;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public interface ILeaseHaulerRequestSendAppService
    {
        Task<bool> SendRequests(SendRequestsInput input);
    }
}