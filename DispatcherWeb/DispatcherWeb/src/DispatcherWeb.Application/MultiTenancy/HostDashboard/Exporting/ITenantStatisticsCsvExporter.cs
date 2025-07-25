using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public interface ITenantStatisticsCsvExporter
    {
        Task<FileDto> ExportToFileAsync(GetTenantStatisticsResult tenantStatisticsResult);
    }
}
