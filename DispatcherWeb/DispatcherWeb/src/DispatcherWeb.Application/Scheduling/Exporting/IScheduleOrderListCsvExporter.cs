using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Scheduling.Dto;

namespace DispatcherWeb.Scheduling.Exporting;

public interface IScheduleOrderListCsvExporter
{
    Task<FileDto> ExportToFileAsync(List<ExportScheduleOrderDto> scheduleOrderDtos);
}
