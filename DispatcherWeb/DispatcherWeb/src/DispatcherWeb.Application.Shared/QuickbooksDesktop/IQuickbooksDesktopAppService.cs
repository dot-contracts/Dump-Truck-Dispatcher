using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Dto;
using DispatcherWeb.QuickbooksDesktop.Dto;

namespace DispatcherWeb.QuickbooksDesktop
{
    public interface IQuickbooksDesktopAppService : IApplicationService
    {
        Task<FileDto> ExportInvoicesToIIF(ExportInvoicesToIIFInput input);
    }
}
