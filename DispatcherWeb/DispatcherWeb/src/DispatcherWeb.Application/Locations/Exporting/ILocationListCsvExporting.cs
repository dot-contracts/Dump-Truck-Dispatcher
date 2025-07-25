using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations.Exporting
{
    public interface ILocationListCsvExporting
    {
        Task<FileDto> ExportToFileAsync(List<LocationDto> locationDtos);
    }
}
