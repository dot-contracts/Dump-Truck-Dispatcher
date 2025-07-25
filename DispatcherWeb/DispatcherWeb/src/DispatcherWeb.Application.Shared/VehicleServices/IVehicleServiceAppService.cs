using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.VehicleServices.Dto;

namespace DispatcherWeb.VehicleServices
{
    public interface IVehicleServiceAppService
    {
        Task<PagedResultDto<VehicleServiceDto>> GetPagedList(GetVehicleServicesInput input);
        Task<VehicleServiceEditDto> GetForEdit(NullableIdDto input);
        Task<VehicleServiceDocumentEditDto> SaveDocument(VehicleServiceDocumentEditDto model);
        Task<VehicleServiceEditDto> Save(VehicleServiceEditDto model);
    }
}