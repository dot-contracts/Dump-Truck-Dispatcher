using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public interface IRequestsCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<RequestDto> dispatchDtos);
    }
}
