using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Chat.Exporting
{
    public interface IChatMessageListExcelExporter
    {
        Task<FileDto> ExportToFileAsync(UserIdentifier user, List<ChatMessageExportDto> messages);
    }
}
