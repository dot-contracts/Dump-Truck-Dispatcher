using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.DriverMessages.Dto;

namespace DispatcherWeb.DriverMessages
{
    public interface IDriverMessageAppService
    {
        Task<PagedResultDto<DriverMessageDto>> GetDriverMessagePagedList(GetDriverMessagePagedListInput input);
        Task SendMessage(SendMessageInput input);
        Task<DriverMessageViewDto> GetForView(int id);
    }
}