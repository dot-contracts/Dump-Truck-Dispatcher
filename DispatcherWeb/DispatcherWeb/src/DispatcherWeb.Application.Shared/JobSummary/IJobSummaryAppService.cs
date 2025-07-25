using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.JobSummary.Dto;

namespace DispatcherWeb.JobSummary
{
    public interface IJobSummaryAppService : IApplicationService
    {
        Task<JobSummaryHeaderDetailsDto> GetJobSummaryHeaderDetails(int orderLineId);
        Task<OrderTrucksDto> GetJobSummaryLoads(int orderLineId);
    }
}
