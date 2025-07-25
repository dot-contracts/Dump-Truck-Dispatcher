using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.ActiveReports.TenantStatisticsReport.Dto;

namespace DispatcherWeb.ActiveReports.TenantStatisticsReport
{
    public interface ITenantStatisticsReportAppService : IApplicationService
    {
        Task<List<TenantDailyHistorySummaryDto>> GetTenantStatistics(GetTenantStatisticsInput input);
    }
}
