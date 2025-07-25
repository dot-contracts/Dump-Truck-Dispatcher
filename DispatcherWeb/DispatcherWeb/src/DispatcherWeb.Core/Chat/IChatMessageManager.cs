using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;
using DispatcherWeb.Chat.Dto;

namespace DispatcherWeb.Chat
{
    public interface IChatMessageManager : IDomainService
    {
        Task<ChatMessage> SendMessageAsync(SendMessageInput input);

        Task<long> SaveAsync(ChatMessage message);

        Task<int> GetUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier sender);

        Task<ChatMessage> FindMessageAsync(int id, long userId);
        Task MarkAsReadAsync(long targetUserId);
    }
}
