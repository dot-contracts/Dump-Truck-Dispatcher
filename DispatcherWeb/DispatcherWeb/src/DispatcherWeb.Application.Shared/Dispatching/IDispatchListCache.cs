using DispatcherWeb.Caching;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Dispatching
{
    public interface IDispatchListCache : IListCache<ListCacheDateKey, DispatchCacheItem>
    {
    }
}
