using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.DemoData
{
    public interface IDemoDataAppService : IApplicationService
    {
        Task CreateDemoData(EntityDto input);
    }
}
