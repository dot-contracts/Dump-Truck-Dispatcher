using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Offices
{
    public interface IOfficeAppService : IApplicationService
    {
        Task<ListResultDto<OfficeDto>> GetAllOffices();
        Task<PagedResultDto<OfficeDto>> GetOffices(GetOfficesInput input);
        Task<PagedResultDto<SelectListDto>> GetOfficesSelectList(GetSelectListInput input);
        Task<OfficeEditDto> GetOfficeForEdit(NullableIdDto input);
        Task<OfficeEditDto> EditOffice(OfficeEditDto model);
        Task<bool> CanDeleteOffice(EntityDto input);
        Task DeleteOffice(EntityDto input);
        Task<int> GetOfficesNumber();
    }
}
