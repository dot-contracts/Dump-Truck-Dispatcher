using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.CannedTexts.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.CannedTexts
{
    public interface ICannedTextAppService : IApplicationService
    {
        Task<PagedResultDto<CannedTextDto>> GetCannedTexts(GetCannedTextsInput input);
        Task<PagedResultDto<SelectListDto>> GetCannedTextsSelectList(GetSelectListInput input);
        Task<CannedTextEditDto> GetCannedTextForEdit(NullableIdDto input);
        Task EditCannedText(CannedTextEditDto model);
        Task DeleteCannedText(EntityDto input);
    }
}
