using System.Threading.Tasks;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckTelematicsAppService
    {
        Task UpdateMileageForAllTenantsAsync();
        Task<bool> IsGpsIntegrationConfigured();
        Task<bool> IsDtdTrackerConfigured();
        Task<bool> IsIntelliShiftConfigured();
        Task<(int trucksUpdated, int trucksIgnored)> UpdateMileageForCurrentTenantAsync(bool continueOnError = false);
        Task SyncWialonDeviceTypesInternal();
        Task UploadTruckPositionsToWialonAsync();
        Task SyncWithIntelliShift();
        Task<SyncWithWialonResult> SyncWithWialon(SyncWithWialonInput input);
    }
}
