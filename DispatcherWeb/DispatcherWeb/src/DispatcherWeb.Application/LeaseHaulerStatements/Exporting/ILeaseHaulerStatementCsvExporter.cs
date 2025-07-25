using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerStatements.Dto;

namespace DispatcherWeb.LeaseHaulerStatements.Exporting
{
    public interface ILeaseHaulerStatementCsvExporter : ICsvExporter
    {
        Task<FileDto> ExportToFileAsync(LeaseHaulerStatementReportDto data);

        Task<FileBytesDto> ExportToFileBytes(LeaseHaulerStatementReportDto data);
    }
}
