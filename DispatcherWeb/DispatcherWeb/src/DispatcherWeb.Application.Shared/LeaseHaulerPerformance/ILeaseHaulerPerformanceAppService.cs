using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.LeaseHaulerPerformance.Dto;

namespace DispatcherWeb.LeaseHaulerPerformance
{
    public interface ILeaseHaulerPerformanceAppService : IApplicationService
    {
        Task<PagedResultDto<LeaseHaulerPerformanceDto>> GetLeaseHaulerPerformances(GetLeaseHaulerPerformancesInput input);
    }
}
