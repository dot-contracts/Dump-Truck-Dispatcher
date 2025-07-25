using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers
{
    public interface IDriverAppService : IApplicationService
    {
        Task<PagedResultDto<DriverDto>> GetDrivers(GetDriversInput input);
        Task<PagedResultDto<SelectListDto>> GetDriversSelectList(GetDriversSelectListInput input);
        Task<DriverEditDto> GetDriverForEdit(NullableIdNameDto input);
        Task<EditDriverResult> EditDriver(DriverEditDto model);
        Task DeleteDriver(EntityDto input);
        Task<PagedResultDto<SelectListDto>> GetDriversFromOrderLineSelectList(GetDriversFromOrderLineSelectListInput input);
        Task<List<DriverCompanyDto>> GetCompanyListForUserDrivers(GetCompanyListForUserDriversInput input);
    }
}
