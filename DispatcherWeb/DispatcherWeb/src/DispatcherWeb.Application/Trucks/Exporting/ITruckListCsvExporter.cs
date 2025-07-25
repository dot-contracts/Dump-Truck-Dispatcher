using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks.Exporting
{
    public interface ITruckListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<TruckEditDto> truckEditDtos);
    }
}
