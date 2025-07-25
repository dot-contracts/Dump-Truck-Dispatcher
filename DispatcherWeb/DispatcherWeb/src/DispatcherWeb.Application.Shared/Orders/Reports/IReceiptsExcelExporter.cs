using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders.Reports
{
    public interface IReceiptsExcelExporter
    {
        Task<FileDto> ExportToFileAsync(List<ReceiptExcelReportDto> list);
    }
}
