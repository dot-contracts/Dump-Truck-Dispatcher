using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Items.Dto;

namespace DispatcherWeb.Items.Exporting
{
    public interface IItemListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<ItemDto> items);
    }
}
