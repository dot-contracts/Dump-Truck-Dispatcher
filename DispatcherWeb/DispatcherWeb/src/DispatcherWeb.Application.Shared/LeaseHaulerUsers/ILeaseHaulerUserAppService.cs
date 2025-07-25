using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulerUsers
{
    public interface ILeaseHaulerUserAppService : IApplicationService
    {
        Task<LeaseHaulerDto> GetLeaseHaulerByUser();
        Task UpdateLeaseHaulerUser(int? leaseHaulerId, long? userId, int? tenantId);
    }
}
