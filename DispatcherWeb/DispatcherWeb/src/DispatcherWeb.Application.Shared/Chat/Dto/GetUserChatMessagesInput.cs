using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Chat.Dto
{
    public class GetUserChatMessagesInput : PagedInputDto
    {
        public int? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        public long? MinMessageId { get; set; }
    }
}
