using DispatcherWeb.Caching;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets
{
    public interface ITicketListCache : IListCache<ListCacheDateKey, TicketCacheItem>
    {
    }
}
