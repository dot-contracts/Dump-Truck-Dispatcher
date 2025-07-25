using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dto;
using DispatcherWeb.Logging.Dto;

namespace DispatcherWeb.Logging
{
    public interface IWebLogAppService : IApplicationService
    {
        Task<GetLatestWebLogsOutput> GetLatestWebLogs();

        Task<FileDto> DownloadWebLogs();
    }
}
