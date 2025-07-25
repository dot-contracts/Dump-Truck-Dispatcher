using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Dispatching.Exporting
{
    public class DispatchListCsvExporter : CsvExporterBase, IDispatchListCsvExporter
    {
        public DispatchListCsvExporter(
            ITempFileCacheManager tempFileCacheManager
        ) : base(tempFileCacheManager)
        {

        }

        public async Task<FileDto> ExportToFileAsync(List<DispatchListDto> dispatchDtos)
        {
            return await CreateCsvFileAsync(
                "LoadHistoryList.csv",
                () =>
                {
                    AddHeaderAndData(
                        dispatchDtos,
                        ("Truck", x => x.TruckCode),
                        ("Driver", x => x.DriverLastFirstName),
                        ("Sent", x => x.Sent?.ToString("g")),
                        ("Acknowledged", x => x.Acknowledged?.ToString("g")),
                        ("Loaded", x => x.Loaded?.ToString("g")),
                        ("Delivered", x => x.Delivered?.ToString("g")),
                        ("Customer", x => x.CustomerName),
                        ("Quote Name", x => x.QuoteName),
                        ("Job Nbr", x => x.JobNumber),
                        ("Load At", x => x.LoadAtName),
                        ("Deliver To", x => x.DeliverToName),
                        ("Item", x => x.Item)
                    );
                }
            );
        }

    }
}
