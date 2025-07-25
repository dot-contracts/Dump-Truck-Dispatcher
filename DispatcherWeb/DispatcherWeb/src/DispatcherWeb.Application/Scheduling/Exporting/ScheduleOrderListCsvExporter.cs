using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Features;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Scheduling.Dto;

namespace DispatcherWeb.Scheduling.Exporting
{
    public class ScheduleOrderListCsvExporter : CsvExporterBase, IScheduleOrderListCsvExporter
    {
        private readonly IFeatureChecker _featureChecker;

        public ScheduleOrderListCsvExporter(
            IFeatureChecker featureChecker,
            ITempFileCacheManager tempFileCacheManager
            )
            : base(tempFileCacheManager)
        {
            _featureChecker = featureChecker;
        }

        public async Task<FileDto> ExportToFileAsync(List<ExportScheduleOrderDto> scheduleOrderDtos)
        {
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            return await CreateCsvFileAsync(
                "ScheduleOrders.csv",
                () =>
                {
                    AddHeaderAndData(
                        scheduleOrderDtos,
                        ("DriverName", x => x.DriverName),
                        ("TruckCode", x => x.TruckCode),
                        ("DeliveryDate", x => x.DeliveryDate.ToString("MM/dd/yyyy")),
                        ("TimeOnJob", x => x.TimeOnJob),
                        ("JobNumber", x => x.JobNumber),
                        ("Customer", x => x.Customer),
                        ("StartName", x => x.StartName),
                        ("StartAddress", x => x.StartAddress),
                        ("DeliverTo", x => x.DeliverTo),
                        ("DeliverToAddress", x => x.DeliverToAddress),
                        (separateItems ? "Freight Item" : "Item", x => x.FreightItemName),
                        (separateItems ? "Material Item" : null, x => x.MaterialItemName),
                        ("Freight Rate", x => x.FreightPricePerUnit?.ToString("N")),
                        ("Material Rate", x => x.MaterialPricePerUnit?.ToString("N")),
                        ("Charge To", x => x.ChargeTo),
                        ("Contact", x => x.Contact),
                        ("AdditionalNotes", x => x.AdditionalNotes));
                });
        }
    }
}
