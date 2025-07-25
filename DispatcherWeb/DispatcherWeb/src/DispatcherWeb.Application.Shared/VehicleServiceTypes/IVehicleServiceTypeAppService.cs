using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.VehicleServiceTypes.Dto;

namespace DispatcherWeb.VehicleServiceTypes
{
    public interface IVehicleServiceTypeAppService
    {
        Task<IList<VehicleServiceTypeDto>> GetList();
        Task<VehicleServiceTypeDto> Save(VehicleServiceTypeDto model);
        Task<bool> Delete(int id);
        Task<PagedResultDto<SelectListDto>> GetSelectList(GetSelectListInput input);
    }
}