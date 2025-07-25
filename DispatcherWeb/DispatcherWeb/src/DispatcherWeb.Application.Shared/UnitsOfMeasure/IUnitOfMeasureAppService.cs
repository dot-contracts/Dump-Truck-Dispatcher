using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.UnitsOfMeasure.Dto;

namespace DispatcherWeb.UnitsOfMeasure
{
    public interface IUnitOfMeasureAppService : IApplicationService
    {
        Task<PagedResultDto<SelectListDto>> GetUnitsOfMeasureSelectList(GetUnitsOfMeasureSelectListInput input);
    }
}
