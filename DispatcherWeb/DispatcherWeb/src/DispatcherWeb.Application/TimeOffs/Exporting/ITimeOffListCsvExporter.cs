using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.TimeOffs.Dto;

namespace DispatcherWeb.TimeOffs.Exporting
{
    public interface ITimeOffListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<TimeOffDto> timeOffDtos);
    }
}
