using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public class RequestsCsvExporter : CsvExporterBase, IRequestsCsvExporter
    {
        public RequestsCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<RequestDto> requestDtos)
        {
            return await CreateCsvFileAsync(
                "Requests.csv",
                () =>
                {
                    AddHeaderAndData(
                        requestDtos,
                        ("Request name", x => $"{x.ServiceName}.{x.MethodName}"),
                        ("Ave. Exec. Time", x => x.AverageExecutionDuration.ToString("N0")),
                        ("Number", x => x.NumberOfTransactions.ToString("N0"))
                    );
                }
            );
        }

    }
}
