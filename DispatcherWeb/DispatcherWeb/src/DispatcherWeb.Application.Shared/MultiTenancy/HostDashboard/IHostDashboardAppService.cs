using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard
{
    public interface IHostDashboardAppService : IApplicationService
    {
        Task<HostDashboardData> GetDashboardStatisticsData(GetDashboardDataInput input);


        Task<TopStatsData> GetTopStatsData(GetTopStatsInput input);
        Task<GetRecentTenantsOutput> GetRecentTenantsData();

        Task<GetExpiringTenantsOutput> GetSubscriptionExpiringTenantsData();

        Task<GetTenantStatisticsResult> GetTenantStatistics(GetDashboardDataInput input);
        Task<PagedResultDto<RequestDto>> GetRequests(GetRequestsInput input);
        Task<HostDashboardKpiDto> GetDashboardKpiData(GetDashboardDataInput input);
    }
}
