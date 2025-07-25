using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.TimeOffs.Dto;

namespace DispatcherWeb.TimeOffs
{
    public interface ITimeOffAppService : IApplicationService
    {
        Task<TimeOffEditDto> GetTimeOffForEdit(NullableIdDto nullableIdDto);
    }
}
