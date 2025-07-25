using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.TimeClassifications.Dto;

namespace DispatcherWeb.TimeClassifications
{
    public interface ITimeClassificationAppService : IApplicationService
    {
        Task<TimeClassificationEditDto> GetTimeClassificationForEdit(NullableIdDto nullableIdDto);
    }
}
