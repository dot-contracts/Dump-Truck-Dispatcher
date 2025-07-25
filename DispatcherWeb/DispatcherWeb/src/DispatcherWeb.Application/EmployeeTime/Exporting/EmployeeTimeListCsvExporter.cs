using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.EmployeeTime.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.EmployeeTime.Exporting
{
    public class EmployeeTimeListCsvExporter : CsvExporterBase, IEmployeeTimeListCsvExporter
    {
        public EmployeeTimeListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<EmployeeTimeDto> employeeTimeDtos)
        {
            return await CreateCsvFileAsync(
                "EmployeeTimeList.csv",
                () =>
                {
                    AddHeaderAndData(
                        employeeTimeDtos,
                        (L("Employee"), x => x.EmployeeName),
                        (L("StartDateTime"), x => x.StartDateTime.ToString("f")),
                        (L("EndDateTime"), x => x.EndDateTime?.ToString("f")),
                        (L("TimeClassification"), x => x.TimeClassificationName),
                        (L("ElapsedTimeHr"), x => x.ElapsedHours.ToString("N2"))
                    );

                }
            );
        }
    }
}
