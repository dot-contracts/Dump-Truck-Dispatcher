using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders.Reports
{
    public class ReceiptsExcelExporter : CsvExporterBase, IReceiptsExcelExporter
    {
        private readonly FeatureChecker _featureChecker;
        private readonly ISettingManager _settingManager;

        public ReceiptsExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            FeatureChecker featureChecker,
            ISettingManager settingManager)
            : base(tempFileCacheManager)
        {
            _featureChecker = featureChecker;
            _settingManager = settingManager;
        }
        public async Task<FileDto> ExportToFileAsync(List<ReceiptExcelReportDto> orderList)
        {
            var currencyCulture = await _settingManager.GetCurrencyCultureAsync();
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            return await CreateCsvFileAsync(
                "Receipts.csv",
                () =>
                {
                    var flatData = orderList.SelectMany(x => x.Items, (order, orderLine) => new
                    {
                        IsFirstRow = order.Items.IndexOf(orderLine) == 0,
                        Order = order,
                        OrderLine = orderLine,
                    }).ToList();

                    AddHeaderAndData(
                        flatData,
                        ("Delivery Date", x => x.IsFirstRow ? x.Order.DeliveryDate.ToString("d") : ""),
                        ("Customer", x => x.IsFirstRow ? x.Order.CustomerName : ""),
                        ("Sales Tax", x => x.IsFirstRow ? x.Order.SalesTax.ToString("C2", currencyCulture) : ""),
                        ("Total", x => x.IsFirstRow ? x.Order.CODTotal.ToString("C2", currencyCulture) : ""),
                        (separateItems ? "Freight" : "Item", x => x.OrderLine.FreightItemName),
                        (separateItems ? "Material" : null, x => x.OrderLine.MaterialItemName),
                        ("Load At", x => x.OrderLine.LoadAtName),
                        ("Deliver To", x => x.OrderLine.DeliverToName),
                        ("Material UOM", x => x.OrderLine.MaterialUomName),
                        ("Freight UOM", x => x.OrderLine.FreightUomName),
                        ("Designation", x => x.OrderLine.DesignationName),
                        ("Actual Material Quantity", x => x.OrderLine.ActualMaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding)),
                        ("Actual Freight Quantity", x => x.OrderLine.ActualFreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding)),
                        ("Freight Amount", x => x.OrderLine.FreightPrice.ToString("C2", currencyCulture)),
                        ("Material Amount", x => x.OrderLine.MaterialPrice.ToString("C2", currencyCulture))
                    );
                });
        }
    }
}
