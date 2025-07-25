using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataServices
{
    public interface IRevenueGraphDataService : IApplicationService
    {
        Task<GetRevenueGraphDataOutput> GetRevenueGraphData(PeriodInput input);
    }
}