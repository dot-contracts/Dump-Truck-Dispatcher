using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.QuickbooksOnline.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public interface IQuickbooksOnlineCsvExporterBase
    {
        Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> recordsList, string filename);
    }
}
