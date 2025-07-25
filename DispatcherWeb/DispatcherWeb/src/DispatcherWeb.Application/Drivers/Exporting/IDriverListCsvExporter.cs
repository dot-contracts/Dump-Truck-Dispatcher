using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Exporting
{
    public interface IDriverListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<DriverDto> driverDtos);
    }
}
