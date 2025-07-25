using DispatcherWeb.Caching;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Dispatching
{
    public interface ILoadListCache : IListCache<ListCacheDateKey, LoadCacheItem>
    {
    }
}
