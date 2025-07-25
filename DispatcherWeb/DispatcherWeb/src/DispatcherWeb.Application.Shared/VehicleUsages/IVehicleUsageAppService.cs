using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.VehicleUsages.Dto;

namespace DispatcherWeb.VehicleUsages
{
    public interface IVehicleUsageAppService
    {
        Task<PagedResultDto<VehicleUsageDto>> GetVehicleUsagePagedList(GetVehicleUsagePagedListInput input);
        Task<VehicleUsageEditDto> SaveVehicleUsage(VehicleUsageEditDto model);
        Task<VehicleUsageEditDto> GetVehicleUsageForEdit(NullableIdDto input);
        Task DeleteVehicleUsage(EntityDto input);
    }
}
