using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Chat.Dto;

namespace DispatcherWeb.Chat
{
    public interface IChatMessageDriverDetailsEnricher
    {
        Task<T> EnrichDriverDetails<T>(T chatMessage) where T : class, IChatMessageWithDriverDetails;
        Task EnrichDriverDetails(IEnumerable<IChatMessageWithDriverDetails> chatMessages);
    }
}
