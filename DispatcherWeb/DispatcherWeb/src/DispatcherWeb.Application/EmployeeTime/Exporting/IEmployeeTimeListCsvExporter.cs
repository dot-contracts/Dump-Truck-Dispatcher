using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;

namespace DispatcherWeb.EmployeeTime.Exporting
{
    public interface IEmployeeTimeListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<EmployeeTimeDto> employeeTimeDtos);
    }
}
