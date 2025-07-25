using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Items.Dto;

namespace DispatcherWeb.Items.Exporting
{
    public class ItemListCsvExporter : CsvExporterBase, IItemListCsvExporter
    {
        public ItemListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<ItemDto> items)
        {
            return await CreateCsvFileAsync(
                "ItemList.csv",
                () =>
                {
                    AddHeaderAndData(
                        items,
                        ("Name", x => x.Name),
                        ("Description", x => x.Description),
                        ("Active", x => x.IsActive.ToYesNoString())
                    );
                }
            );
        }

    }
}
