using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.HostEmails.Dto;

namespace DispatcherWeb.HostEmails
{
    public interface IHostEmailAppService : IApplicationService
    {
        Task<HostEmailViewDto> GetHostEmailForView(EntityDto input);
    }
}
