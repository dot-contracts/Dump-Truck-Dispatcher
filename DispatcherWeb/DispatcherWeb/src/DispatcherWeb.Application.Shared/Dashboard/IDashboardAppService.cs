using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dashboard.Dto;

namespace DispatcherWeb.Dashboard
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<GetTruckUtilizationDataOutput> GetTruckUtilizationData(GetTruckUtilizationDataInput input);
        Task<ScheduledTruckCountDto> GetScheduledTruckCountDto();
        Task<GetTruckAvailabilityDataOutput> GetTruckAvailabilityData();
        Task<GetTenantDashboardStatusDataOutput> GetTruckServiceStatusData();
        Task<GetTenantDashboardStatusDataOutput> GetTruckLicensePlateStatusData();
        Task<GetTenantDashboardStatusDataOutput> GetDriverLicenseStatusData();
        Task<GetTenantDashboardStatusDataOutput> GetDriverPhysicalStatusData();
        Task<GetTenantDashboardStatusDataOutput> GetDriverMVRStatusData();
        Task<RevenueChartsDataDto> GetRevenueChartsData(GetRevenueChartsDataInput input);
        Task<List<DashboardSettingDto>> GetDashboardSettings();
    }
}
