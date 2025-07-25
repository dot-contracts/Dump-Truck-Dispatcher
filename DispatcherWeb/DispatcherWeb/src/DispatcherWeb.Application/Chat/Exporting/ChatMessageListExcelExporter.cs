using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Chat.Exporting
{
    public class ChatMessageListExcelExporter : CsvExporterBase, IChatMessageListExcelExporter
    {
        public ChatMessageListExcelExporter(
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(UserIdentifier user, List<ChatMessageExportDto> messages)
        {
            var timezone = await GetTimezone();
            var tenancyName = messages.Count > 0 ? messages.First().TargetTenantName : L("Anonymous");
            var userName = messages.Count > 0 ? messages.First().TargetUserName : L("Anonymous");

            return await CreateCsvFileAsync(
                $"Chat_{tenancyName}_{userName}.csv",
                () =>
                {
                    AddHeaderAndData(
                        messages,
                        (L("ChatMessage_From"), x => x.Side == ChatSide.Receiver ? (x.TargetTenantName + "/" + x.TargetUserName) : L("You")),
                        (L("ChatMessage_To"), x => x.Side == ChatSide.Receiver ? L("You") : (x.TargetTenantName + "/" + x.TargetUserName)),
                        (L("Message"), x => x.Message),
                        (L("ReadState"), x => x.Side == ChatSide.Receiver ? x.ReadState.ToString() : x.ReceiverReadState.ToString()),
                        (L("CreationTime"), x => x.CreationTime.ConvertTimeZoneTo(timezone).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"))
                    );
                });
        }
    }
}
