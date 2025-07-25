using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Dispatching.Exporting
{
    public interface IDispatchListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<DispatchListDto> dispatchDtos);
    }
}
